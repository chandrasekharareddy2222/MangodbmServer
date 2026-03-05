using ExcelDataReader;
using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FieldMetadataAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/check-table-value")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class CheckTableValueController : ControllerBase
    {
        private readonly ICheckTableValueService _service;
        private readonly ILogger<CheckTableValueController> _logger;
        private readonly IValidator<CreateCheckTableValueDto> _createValidator;
        private readonly IValidator<UpdateCheckTableValueDto> _updateValidator;
        private readonly IValidator<CheckTableValueImportRowDto> _rowValidator;

        public CheckTableValueController(
            ICheckTableValueService service,
            ILogger<CheckTableValueController> logger,
            IValidator<CreateCheckTableValueDto> createValidator,
            IValidator<UpdateCheckTableValueDto> updateValidator,
            IValidator<CheckTableValueImportRowDto> rowValidator)
        {
            _service = service;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _rowValidator = rowValidator;
        }

        /// <summary>
        /// Get Check Table Values
        /// Example:
        /// api/CheckTableValue?tableName=T134
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] CheckTableQueryDto query)
        {
            // If invalid, FluentValidation auto returns 400 and it will not come here
            var result = await _service.GetByTableNameAsync(query.TableName!);

            if (!result.Any())
                return NotFound("No records found.");

            return Ok(result);
        }

        /// <summary>
        /// Creates a new Check Table Value record
        /// 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCheckTableValueDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var id = await _service.CreateAsync(dto);
            return Ok(new { Message = "Record created successfully", CheckTableID = id });
        }

        /// <summary>
        /// Update Check Table Value
        /// api/CheckTableValue
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCheckTableValueDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound("Record not found.");

            return Ok(new { Message = "Record updated successfully" });
        }
        /// <summary>
        /// Soft Delete Check Table Value
        /// api/CheckTableValue
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation(
                "Soft deleting CheckTableValue {Id}", id);

            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound("Record not found.");

            return Ok(new
            {
                Message = "Record deleted successfully"
            });
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string tableName, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest("tableName is required.");

            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var rows = new List<CheckTableValueImportRowDto>();
            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = file.OpenReadStream();

            if (extension == ".csv")
            {
                using var reader = new StreamReader(stream);
                bool isHeader = true;
                int rowNumber = 1;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    rowNumber++;

                    if (isHeader) { isHeader = false; continue; }

                    var cols = (line ?? "").Split(',');

                    rows.Add(new CheckTableValueImportRowDto
                    {
                        KeyValue = cols.ElementAtOrDefault(0)?.Trim(),
                        Description = cols.ElementAtOrDefault(1)?.Trim(),
                        AdditionalInfo = cols.ElementAtOrDefault(2)?.Trim(),
                        RowNumber = rowNumber
                    });
                }
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                });

                var table = ds.Tables[0];
                int rowNumber = 1; // header handled by UseHeaderRow
                foreach (DataRow row in table.Rows)
                {
                    rowNumber++;
                    rows.Add(new CheckTableValueImportRowDto
                    {
                        KeyValue = row[0]?.ToString()?.Trim(),
                        Description = row[1]?.ToString()?.Trim(),
                        AdditionalInfo = row[2]?.ToString()?.Trim(),
                        RowNumber = rowNumber
                    });
                }
            }
            else
            {
                return BadRequest("Only .csv, .xls, .xlsx are supported.");
            }

            // Validate rows
            var errors = new List<object>();
            var validRows = new List<CheckTableValueImportRowDto>();

            foreach (var r in rows)
            {
                var v = await _rowValidator.ValidateAsync(r);
                if (!v.IsValid)
                {
                    errors.Add(new
                    {
                        r.RowNumber,
                        Errors = v.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                    });
                    continue; // skip invalid row
                }

                validRows.Add(r);
            }

            // If you want partial insert, DO NOT return 400
            // Just insert valid rows and return errors as "Skipped"
            var dtos = validRows.Select(r => new CreateCheckTableValueDto
            {
                CheckTableName = tableName,
                KeyValue = r.KeyValue,
                Description = r.Description,
                AdditionalInfo = r.AdditionalInfo,
                IsActive = true,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Parse("9999-12-31"),
                CreatedBy = "FILE_IMPORT"
            }).ToList();

            var (inserted, skipped) = await _service.ImportValuesAsync(tableName, dtos);

            return Ok(new
            {
                Message = "Upload completed",
                Inserted = inserted,
                Skipped = errors.Count,
                ValidationErrors = errors
            });
        }
    } 
}
