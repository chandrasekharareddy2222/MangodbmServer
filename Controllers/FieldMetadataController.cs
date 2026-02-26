using Microsoft.AspNetCore.Mvc;
using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Services;
using FieldMetadataAPI.Mappings;
using FieldMetadataAPI.Models;
using FluentValidation;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ExcelDataReader;
using System.Data;

namespace FieldMetadataAPI.Controllers
{
    /// <summary>
    /// API Controller for Field Metadata operations
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/field-metadata")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class FieldMetadataController : ControllerBase
    {
        private readonly IFieldMetadataService _service;
        private readonly ILogger<FieldMetadataController> _logger;
        private readonly IValidator<CreateFieldMetadataDto> _createValidator;
        private readonly IValidator<UpdateFieldMetadataDto> _updateValidator;
        private readonly IValidator<FieldMetadataQueryDto> _queryValidator;
        private readonly IValidator<BulkUpdateMandatoryDto> _bulkUpdateMandatoryValidator;

        public FieldMetadataController(
            IFieldMetadataService service,
            ILogger<FieldMetadataController> logger,
            IValidator<CreateFieldMetadataDto> createValidator,
            IValidator<UpdateFieldMetadataDto> updateValidator,
            IValidator<FieldMetadataQueryDto> queryValidator,
            IValidator<BulkUpdateMandatoryDto> bulkUpdateMandatoryValidator)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _queryValidator = queryValidator ?? throw new ArgumentNullException(nameof(queryValidator));
            _bulkUpdateMandatoryValidator = bulkUpdateMandatoryValidator ?? throw new ArgumentNullException(nameof(bulkUpdateMandatoryValidator));
        }

        /// <summary>
        /// Get all active field metadata with optional filtering and pagination
        /// </summary>
        /// <param name="query">Query parameters for filtering and pagination</param>
        /// <returns>Paginated list of field metadata</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<FieldMetadataDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] FieldMetadataQueryDto query)
        {
            _logger.LogInformation("GET request received for all field metadata");

            var validationResult = await _queryValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            var result = await _service.GetAllAsync(query);
            return Ok(ApiResponse<PagedResponse<FieldMetadataDto>>.SuccessResponse(result, "Field metadata retrieved successfully"));
        }

        /// <summary>
        /// Get field metadata by field name
        /// </summary>
        /// <param name="fieldName">The field name (primary key)</param>
        /// <returns>Field metadata details</returns>
        [HttpGet("{fieldName}")]
        [ProducesResponseType(typeof(ApiResponse<FieldMetadataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string fieldName)
        {
            _logger.LogInformation("GET request received for field metadata: {FieldName}", fieldName);

            var result = await _service.GetByIdAsync(fieldName);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Field metadata with FieldName '{fieldName}' not found"));
            }

            return Ok(ApiResponse<FieldMetadataDto>.SuccessResponse(result, "Field metadata retrieved successfully"));
        }

        /// <summary>
        /// Get all field metadata with check table values and passable values
        /// </summary>
        /// <returns>List of field metadata with associated lookup values</returns>
        [HttpGet("with-values")]
        [ProducesResponseType(typeof(ApiResponse<List<FieldMetadataWithValuesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllWithValues()
        {
            _logger.LogInformation("GET request received for all field metadata with values");

            var result = await _service.GetAllWithValuesAsync();
            return Ok(ApiResponse<List<FieldMetadataWithValuesDto>>.SuccessResponse(result, "Field metadata with values retrieved successfully"));
        }

        /// <summary>
        /// Create new field metadata
        /// </summary>
        /// <param name="createDto">Field metadata creation details</param>
        /// <returns>Created field metadata</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FieldMetadataDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateFieldMetadataDto createDto)
        {
            _logger.LogInformation("POST request received to create field metadata");

            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            var result = await _service.CreateAsync(createDto);
            return CreatedAtAction(
                nameof(GetById),
                new { fieldName = result.FieldName },
                ApiResponse<FieldMetadataDto>.SuccessResponse(result, "Field metadata created successfully")
            );
        }

        /// <summary>
        /// Update field metadata (only editable fields)
        /// </summary>
        /// <param name="fieldName">The field name (primary key)</param>
        /// <param name="updateDto">Field metadata update details</param>
        /// <returns>Success indicator</returns>
        [HttpPut("{fieldName}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string fieldName, [FromBody] UpdateFieldMetadataDto updateDto)
        {
            _logger.LogInformation("PUT request received to update field metadata: {FieldName}", fieldName);

            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            var result = await _service.UpdateAsync(fieldName, updateDto);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Field metadata with FieldName '{fieldName}' not found"));
            }

            return Ok(ApiResponse<object?>.SuccessResponse(null, "Field metadata updated successfully"));
        }

        /// <summary>
        /// Bulk update IsMandatory field for multiple field metadata records
        /// </summary>
        /// <param name="bulkUpdateDto">List of field updates with individual IsMandatory values</param>
        /// <returns>Number of records updated</returns>
        [HttpPatch("bulk-update-mandatory")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkUpdateMandatory([FromBody] BulkUpdateMandatoryDto bulkUpdateDto)
        {
            _logger.LogInformation("PATCH request received to bulk update IsMandatory for {Count} field(s)", bulkUpdateDto.Updates.Count);

            var validationResult = await _bulkUpdateMandatoryValidator.ValidateAsync(bulkUpdateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            var rowsAffected = await _service.BulkUpdateMandatoryAsync(bulkUpdateDto);

            if (rowsAffected == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("No records were updated. Verify that the field names exist and are active."));
            }

            var message = $"Successfully updated {rowsAffected} field metadata record(s).";
            return Ok(ApiResponse<object>.SuccessResponse(new { rowsAffected, updatesProcessed = bulkUpdateDto.Updates.Count }, message));
        }

        /// <summary>
        /// Soft delete field metadata (sets IsActive = 0)
        /// </summary>
        /// <param name="fieldName">The field name (primary key)</param>
        /// <returns>Success indicator</returns>
        [HttpDelete("{fieldName}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string fieldName)
        {
            _logger.LogInformation("DELETE request received for field metadata: {FieldName}", fieldName);

            var result = await _service.DeleteAsync(fieldName);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Field metadata with FieldName '{fieldName}' not found"));
            }

            return Ok(ApiResponse<object?>.SuccessResponse(null, "Field metadata deleted successfully"));
        }



        /// <summary>
        /// Import field metadata from a CSV file
        /// </summary>
        /// <param name="file">CSV file containing field metadata records</param>
        /// <returns>Import summary with success/failure counts</returns>
        [HttpPost("import")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            _logger.LogInformation("POST request received to import CSV or EXCEL file: {FileName}", file?.FileName);

            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("No file provided or file is empty"));
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".csv" && extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("File must be a CSV or Excel file"));
            }

            try
            {
                var csvRows = new List<CsvImportRow>();

                using (var stream = file.OpenReadStream())
                {
                    if (extension == ".csv")
                    {
                        using (var reader = new StreamReader(stream))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Context.RegisterClassMap<CsvImportRowMap>();
                            try
                            {
                                csvRows = csv.GetRecords<CsvImportRow>().ToList();
                            }
                            catch (CsvHelper.BadDataException ex)
                            {
                                _logger.LogError(ex, "CSV or EXCEL header validation failed");
                                return BadRequest(ApiResponse<object>.ErrorResponse($"CSV or EXCEL header format error: {ex.Message}"));
                            }
                        }
                    }
                    else // Excel (.xlsx or .xls)
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                        using (var excelReader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = excelReader.AsDataSet(new ExcelDataSetConfiguration
                            {
                                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                                {
                                    UseHeaderRow = true
                                }
                            });
                            var table = result.Tables[0];
                            var subjectIndex = table.Columns
                                .Cast<DataColumn>()
                                .FirstOrDefault(c => c.ColumnName.Trim().Equals("Subject", StringComparison.OrdinalIgnoreCase))
                                ?.Ordinal ?? -1;

                            for (int i = 0; i < table.Rows.Count; i++) // skip header
                            {
                                var row = table.Rows[i];

                                csvRows.Add(new CsvImportRow
                                {
                                    Field = row["Field"]?.ToString(),
                                    KeyField = row["Key Field"]?.ToString(),
                                    DataElement = row["Data element"]?.ToString(),
                                    Datatype = row["Data Type"]?.ToString(),
                                    Length = row["Length"]?.ToString(),
                                    Decimals = row["Decimals"]?.ToString(),
                                    Description = row["Short Description"]?.ToString(),
                                    Checktable = row["Check Table"]?.ToString(),
                                    PossibleValues = row["Possible values"]?.ToString(),
                                    Subject = row.Table.Columns.Contains("Subject")? row["Subject"]?.ToString(): null
                                });
                            }
                        }
                    }
                }

                if (csvRows.Count == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("CSV or EXCEL file contains no records"));
                }

                _logger.LogInformation("CSV or EXCEL file parsed successfully. Total records: {Count}", csvRows.Count);

                // Call service to process CSV with tracking
                var importResponse = await _service.ImportCsvWithTrackingAsync(csvRows, _createValidator);

                _logger.LogInformation("CSV or EXCEL  import completed. Inserted: {Inserted}, Failed: {Failed}, Skipped: {Skipped}", 
                    importResponse.Inserted, importResponse.Failed, importResponse.Skipped);

                // Return JSON response with result file content as base64
                return Ok(ApiResponse<object>.SuccessResponse(
                    new
                    {
                        totalRecords = importResponse.TotalRecords,
                        inserted = importResponse.Inserted,
                        failed = importResponse.Failed,
                        skipped = importResponse.Skipped,
                        resultFileName = $"import_result_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv",
                        resultFileContent = Convert.ToBase64String(importResponse.ResultFileContent),
                        rowResults = importResponse.RowResults.Select(r => new
                        {
                            fieldName = r.FieldName,
                            importStatus = r.ImportStatus,
                            errorCode = r.ErrorCode,
                            errorMessage = r.ErrorMessage
                        }).ToList()
                    },
                    $"CSV or EXCEL  import completed successfully. {importResponse.Inserted} record(s) inserted, {importResponse.Failed} failed, {importResponse.Skipped} skipped."
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV or EXCEL file: {FileName}", file.FileName);
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error processing CSV or EXCEL file: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all active check tables
        /// </summary>
        [HttpGet("active-checktables")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveCheckTables()
        {
            _logger.LogInformation("GET request received for active check tables");

            var result = await _service.GetActiveCheckTablesAsync();

            return Ok(ApiResponse<List<string>>.SuccessResponse(
                result,
                "Active check tables retrieved successfully"));
        }
    }
}
