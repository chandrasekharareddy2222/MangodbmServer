using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;
using FluentValidation;
using System.Text;

namespace FieldMetadataAPI.Services
{
    /// <summary>
    /// Service interface for Field Metadata business logic
    /// </summary>
    public interface IFieldMetadataService
    {
        Task<PagedResponse<FieldMetadataDto>> GetAllAsync(FieldMetadataQueryDto query);
        Task<FieldMetadataDto?> GetByIdAsync(string fieldName);
        Task<FieldMetadataDto> CreateAsync(CreateFieldMetadataDto createDto);
        Task<bool> UpdateAsync(string fieldName, UpdateFieldMetadataDto updateDto);
        Task<bool> DeleteAsync(string fieldName);
        Task<List<FieldMetadataWithValuesDto>> GetAllWithValuesAsync();
        Task<int> BulkUpdateMandatoryAsync(BulkUpdateMandatoryDto bulkUpdateDto);
        Task<CsvImportResponse> ImportCsvWithTrackingAsync(List<CsvImportRow> rows, IValidator<CreateFieldMetadataDto> validator);
        Task<List<string>> GetActiveCheckTablesAsync();
    }

    /// <summary>
    /// Service implementation for Field Metadata business logic
    /// </summary>
    public class FieldMetadataService : IFieldMetadataService
    {
        private readonly IFieldMetadataRepository _repository;
        private readonly ILogger<FieldMetadataService> _logger;

        public FieldMetadataService(IFieldMetadataRepository repository, ILogger<FieldMetadataService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResponse<FieldMetadataDto>> GetAllAsync(FieldMetadataQueryDto query)
        {
            _logger.LogInformation("Getting all field metadata with filters");

            var items = await _repository.GetAllAsync(
                query.FieldName,
                query.TableGroup,
                query.DataType,
                query.PageNumber,
                query.PageSize
            );

            var totalCount = await _repository.GetTotalCountAsync(
                query.FieldName,
                query.TableGroup,
                query.DataType
            );

            var dtoItems = items.Select(MapToDto).ToList();

            return new PagedResponse<FieldMetadataDto>
            {
                Items = dtoItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<FieldMetadataDto?> GetByIdAsync(string fieldName)
        {
            _logger.LogInformation("Getting field metadata by ID: {FieldName}", fieldName);

            var entity = await _repository.GetByIdAsync(fieldName);
            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<FieldMetadataDto> CreateAsync(CreateFieldMetadataDto createDto)
        {
            _logger.LogInformation("Creating new field metadata: {FieldName}", createDto.FieldName);

            // Business Rule: FieldName must be unique
            if (await _repository.ExistsAsync(createDto.FieldName))
            {
                _logger.LogWarning("Field metadata already exists: {FieldName}", createDto.FieldName);
                throw new InvalidOperationException($"Field metadata with FieldName '{createDto.FieldName}' already exists.");
            }

            var entity = new FieldMetadata
            {
                FieldName = createDto.FieldName,
                DataElement = createDto.DataElement,
                Description = createDto.Description,
                KeyField = createDto.KeyField,
                CheckTable = createDto.CheckTable,
                DataType = createDto.DataType,
                FieldLength = createDto.FieldLength,
                Decimals = createDto.Decimals,
                HasDropdown = createDto.HasDropdown,
                TableGroup = createDto.TableGroup,
                IsActive = createDto.IsActive,
                CreatedDate = DateTime.UtcNow
                // ValidationType, IsMandatory, UIControlType are computed - not set here
            };

            await _repository.CreateAsync(entity);

            // Retrieve the created entity to get computed columns
            var created = await _repository.GetByIdAsync(createDto.FieldName);
            return MapToDto(created!);
        }

        public async Task<bool> UpdateAsync(string fieldName, UpdateFieldMetadataDto updateDto)
        {
            _logger.LogInformation("Updating field metadata: {FieldName}", fieldName);

            var existing = await _repository.GetByIdAsync(fieldName);
            if (existing == null)
            {
                _logger.LogWarning("Field metadata not found: {FieldName}", fieldName);
                return false;
            }

            // Update only editable fields
            existing.Description = updateDto.Description;
            existing.CheckTable = updateDto.CheckTable;
            existing.HasDropdown = updateDto.HasDropdown;
            existing.TableGroup = updateDto.TableGroup;
            existing.IsActive = updateDto.IsActive;

            var rowsAffected = await _repository.UpdateAsync(fieldName, existing);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string fieldName)
        {
            _logger.LogInformation("Soft deleting field metadata: {FieldName}", fieldName);

            var existing = await _repository.GetByIdAsync(fieldName);
            if (existing == null)
            {
                _logger.LogWarning("Field metadata not found: {FieldName}", fieldName);
                return false;
            }

            var rowsAffected = await _repository.SoftDeleteAsync(fieldName);
            return rowsAffected > 0;
        }

        private FieldMetadataDto MapToDto(FieldMetadata entity)
        {
            return new FieldMetadataDto
            {
                FieldName = entity.FieldName,
                DataElement = entity.DataElement,
                Description = entity.Description,
                KeyField = entity.KeyField,
                CheckTable = entity.CheckTable,
                DataType = entity.DataType,
                FieldLength = entity.FieldLength,
                Decimals = entity.Decimals,
                ValidationType = entity.ValidationType,
                HasDropdown = entity.HasDropdown,
                IsMandatory = entity.IsMandatory,
                TableGroup = entity.TableGroup,
                UIControlType = entity.UIControlType,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate
            };
        }

        public async Task<List<FieldMetadataWithValuesDto>> GetAllWithValuesAsync()
        {
            _logger.LogInformation("Getting all field metadata with check table and passable values");

            var data = await _repository.GetAllWithValuesAsync();

            var result = data.Select(kvp =>
            {
                var (metadata, checkTableValues, passableValues) = kvp.Value;

                return new FieldMetadataWithValuesDto
                {
                    FieldName = metadata.FieldName,
                    DataElement = metadata.DataElement,
                    Description = metadata.Description,
                    IsMandatory = metadata.IsMandatory,
                    CheckTable = metadata.CheckTable,
                    DataType = metadata.DataType,
                    FieldLength = metadata.FieldLength,
                    Decimals = metadata.Decimals,
                    ValidationType = metadata.ValidationType,
                    HasDropdown = !string.IsNullOrEmpty(metadata.HasDropdown) && metadata.HasDropdown == "X",
                    TableGroup = metadata.TableGroup,
                    UIControlType = metadata.UIControlType,
                    IsActive = metadata.IsActive,
                    CreatedDate = metadata.CreatedDate,
                    ModifiedDate = null, // Add this to FieldMetadata model if needed
                    CheckTableValues = checkTableValues.Any() 
                        ? checkTableValues.Select(ctv => new CheckTableValueDto
                        {
                            TableName = ctv.CheckTableName,
                            KeyValue = ctv.KeyValue,
                            Description = ctv.Description,
                            AdditionalInfo = ctv.AdditionalInfo,
                            IsActive = ctv.IsActive,
                            ValidFrom = ctv.ValidFrom,
                            ValidTo = ctv.ValidTo,
                            CreatedDate = ctv.CreatedDate,
                            CreatedBy = ctv.CreatedBy
                        }).ToList()
                        : null,
                    PassableValues = passableValues.Any()
                        ? passableValues.Select(pv => new PassableValueDto
                        {
                            FieldName = pv.FieldName,
                            Value = pv.KeyValue,
                            DisplayValue = pv.DisplayValue,
                            Description = pv.Description,
                            DisplayOrder = pv.DisplayOrder,
                            IsDefault = pv.IsDefault,
                            IconClass = pv.IconClass,
                            ColorCode = pv.ColorCode,
                            IsActive = pv.IsActive,
                            CreatedDate = pv.CreatedDate
                        }).ToList()
                        : null
                };
            }).ToList();

            return result;
        }

        public async Task<int> BulkUpdateMandatoryAsync(BulkUpdateMandatoryDto bulkUpdateDto)
        {
            _logger.LogInformation("Bulk updating IsMandatory for {Count} field(s)", bulkUpdateDto.Updates.Count);

            // Validate that all field names exist and build the updates list
            var validUpdates = new List<(string FieldName, bool IsMandatory)>();
            var skippedCount = 0;

            foreach (var update in bulkUpdateDto.Updates)
            {
                var exists = await _repository.ExistsAsync(update.FieldName);
                if (exists)
                {
                    validUpdates.Add((update.FieldName, update.IsMandatory));
                }
                else
                {
                    _logger.LogWarning("FieldName '{FieldName}' not found or inactive, skipping", update.FieldName);
                    skippedCount++;
                }
            }

            if (validUpdates.Count == 0)
            {
                _logger.LogWarning("No valid field names found for update");
                return 0;
            }

            var rowsAffected = await _repository.BulkUpdateMandatoryAsync(validUpdates);

            _logger.LogInformation("Successfully updated {RowsAffected} out of {TotalRequested} field(s) ({SkippedCount} skipped)", 
                rowsAffected, bulkUpdateDto.Updates.Count, skippedCount);

            return rowsAffected;
        }

        public async Task<CsvImportResponse> ImportCsvWithTrackingAsync(List<CsvImportRow> rows, IValidator<CreateFieldMetadataDto> validator)
        {
            _logger.LogInformation("Starting CSV import with tracking for {Count} records", rows.Count);

            var response = new CsvImportResponse
            {
                TotalRecords = rows.Count,
                ResultFileName = $"field_metadata_import_result_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv"
            };

            // Get all existing field names to check for duplicates
            var existingFields = await _repository.GetAllFieldNamesAsync();
            var existingFieldsSet = new HashSet<string>(existingFields.Select(f => f.ToUpper()), StringComparer.OrdinalIgnoreCase);

            // Process each row
            foreach (var row in rows)
            {
                var result = new ImportRowResult { FieldName = row.Field };

                try
                {
                    // Check if field name is provided
                    if (string.IsNullOrWhiteSpace(row.Field))
                    {
                        result.ImportStatus = "FAILED";
                        result.ErrorCode = "VALIDATION_ERROR";
                        result.ErrorMessage = "Field name is required";
                        response.Failed++;
                        row.Result = result;
                        continue;
                    }

                    string normalizedFieldName = row.Field.Trim().ToUpper();

                    // Check if row was already successfully imported (re-upload scenario)
                    if (result.ImportStatus == "SUCCESS")
                    {
                        result.ImportStatus = "SKIPPED";
                        result.ErrorCode = "";
                        result.ErrorMessage = "Record previously imported successfully";
                        response.Skipped++;
                        row.Result = result;
                        continue;
                    }

                    // Check for duplicate in database
                    if (existingFieldsSet.Contains(normalizedFieldName))
                    {
                        result.ImportStatus = "FAILED";
                        result.ErrorCode = "DUPLICATE_FIELD";
                        result.ErrorMessage = $"Field metadata with FieldName '{normalizedFieldName}' already exists";
                        response.Failed++;
                        row.Result = result;
                        continue;
                    }

                    // Transform and validate the record
                    var createDto = TransformCsvRowToDto(row, normalizedFieldName);

                    var validationResult = await validator.ValidateAsync(createDto);
                    if (!validationResult.IsValid)
                    {
                        var validationErrors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                        result.ImportStatus = "FAILED";
                        result.ErrorCode = "VALIDATION_ERROR";
                        result.ErrorMessage = validationErrors;
                        response.Failed++;
                        row.Result = result;
                        continue;
                    }

                    // Attempt to insert
                    await _repository.CreateAsync(MapDtoToModel(createDto));
                    existingFieldsSet.Add(normalizedFieldName); // Add to local set to prevent duplicates within same batch

                    result.ImportStatus = "SUCCESS";
                    result.ErrorCode = "";
                    result.ErrorMessage = "";
                    response.Inserted++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing row {FieldName}", row.Field);
                    result.ImportStatus = "FAILED";
                    result.ErrorCode = "SYSTEM_ERROR";
                    result.ErrorMessage = $"Unexpected error: {ex.Message}";
                    response.Failed++;
                }

                row.Result = result;
                response.RowResults.Add(result);
            }

            // Generate result CSV file
            response.ResultFileContent = GenerateResultCsv(rows);
            response.Message = $"CSV import completed. {response.Inserted} record(s) inserted, {response.Failed} failed, {response.Skipped} skipped.";

            _logger.LogInformation("CSV import completed: Inserted={Inserted}, Failed={Failed}, Skipped={Skipped}", 
                response.Inserted, response.Failed, response.Skipped);

            return response;
        }

        private CreateFieldMetadataDto TransformCsvRowToDto(CsvImportRow row, string normalizedFieldName)
        {
            string? dataElement = string.IsNullOrWhiteSpace(row.DataElement?.Trim()) ? null : row.DataElement.Trim();
            string? description = string.IsNullOrWhiteSpace(row.Description?.Trim()) ? null : row.Description.Trim();
            string? keyField = row.KeyField?.Trim().ToUpper() == "X" ? "X" : null;
            string? checkTable = string.IsNullOrWhiteSpace(row.Checktable?.Trim()) ? null : row.Checktable.Trim();
            string? dataType = row.Datatype?.Trim().ToUpper() ?? "";
            int fieldLength = int.TryParse(row.Length, out var len) ? len : 0;
            int decimals = int.TryParse(row.Decimals, out var dec) ? dec : 0;

            // Handle HasDropdown: if PossibleValues contains "Possible values", set to 'X'
            string? hasDropdown = null;
            if (!string.IsNullOrWhiteSpace(row.PossibleValues?.Trim()))
            {
                string normalized = row.PossibleValues.Trim().ToLower();
                if (normalized == "possible values" || normalized == "x")
                {
                    hasDropdown = "X";
                }
            }

            // Auto-categorize TableGroup
            string tableGroup = AutoCategorizeTableGroup(normalizedFieldName, dataType, checkTable);

            return new CreateFieldMetadataDto
            {
                FieldName = normalizedFieldName,
                DataElement = dataElement,
                Description = description,
                KeyField = keyField,
                CheckTable = checkTable,
                DataType = dataType,
                FieldLength = fieldLength,
                Decimals = decimals,
                HasDropdown = hasDropdown,
                TableGroup = tableGroup
            };
        }

        private string AutoCategorizeTableGroup(string fieldName, string dataType, string? checkTable)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                return "General Attributes";

            if (dataType == "QUAN" || dataType == "DEC" || dataType == "INT2")
                return "Quantities";

            if (dataType == "DATS")
                return "Dates";

            if (dataType == "UNIT")
                return "Units of Measure";

            if (!string.IsNullOrWhiteSpace(checkTable) && checkTable.StartsWith("T", StringComparison.OrdinalIgnoreCase))
                return "Master Data";

            if (fieldName.Contains("FIBER", StringComparison.OrdinalIgnoreCase))
                return "Textile Composition";

            if (fieldName.StartsWith("/VSO", StringComparison.OrdinalIgnoreCase))
                return "Vehicle Space Optimization";

            if (fieldName.StartsWith("/CWM", StringComparison.OrdinalIgnoreCase))
                return "Catch Weight Management";

            if (fieldName.StartsWith("/BEV", StringComparison.OrdinalIgnoreCase))
                return "Beverage Industry";

            return "General Attributes";
        }

        private FieldMetadata MapDtoToModel(CreateFieldMetadataDto dto)
        {
            return new FieldMetadata
            {
                FieldName = dto.FieldName,
                DataElement = dto.DataElement,
                Description = dto.Description,
                KeyField = dto.KeyField,
                CheckTable = dto.CheckTable,
                DataType = dto.DataType,
                FieldLength = dto.FieldLength,
                Decimals = dto.Decimals,
                HasDropdown = dto.HasDropdown,
                TableGroup = dto.TableGroup
            };
        }

        private byte[] GenerateResultCsv(List<CsvImportRow> rows)
        {
            var sb = new StringBuilder();

            // Write header with original columns + tracking columns
            sb.AppendLine("Field,Data element,Description,Key Field,Checktable,Datatype,Length,Decimals,Possible values,ImportStatus,ErrorCode,ErrorMessage");

            // Write rows with their results
            foreach (var row in rows)
            {
                var fieldEscaped = EscapeCsvField(row.Field);
                var dataElementEscaped = EscapeCsvField(row.DataElement);
                var descriptionEscaped = EscapeCsvField(row.Description);
                var keyFieldEscaped = EscapeCsvField(row.KeyField);
                var checktableEscaped = EscapeCsvField(row.Checktable);
                var datatypeEscaped = EscapeCsvField(row.Datatype);
                var lengthEscaped = EscapeCsvField(row.Length);
                var decimalsEscaped = EscapeCsvField(row.Decimals);
                var possibleValuesEscaped = EscapeCsvField(row.PossibleValues);
                var statusEscaped = EscapeCsvField(row.Result.ImportStatus);
                var errorCodeEscaped = EscapeCsvField(row.Result.ErrorCode);
                var errorMessageEscaped = EscapeCsvField(row.Result.ErrorMessage);

                sb.AppendLine($"{fieldEscaped},{dataElementEscaped},{descriptionEscaped},{keyFieldEscaped},{checktableEscaped},{datatypeEscaped},{lengthEscaped},{decimalsEscaped},{possibleValuesEscaped},{statusEscaped},{errorCodeEscaped},{errorMessageEscaped}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private string EscapeCsvField(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains("\"") || value.Contains(",") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
        
        public async Task<List<string>> GetActiveCheckTablesAsync()
        {
            _logger.LogInformation("Getting active check tables");

            var result = await _repository.GetActiveCheckTablesAsync();

            return result.ToList();
        }
    }
}
