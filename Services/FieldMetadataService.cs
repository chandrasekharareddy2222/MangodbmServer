    using FieldMetadataAPI.DTOs;
    using FieldMetadataAPI.Models;
    using FieldMetadataAPI.Repositories;
    using FluentValidation;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using FieldMetadataAPI.Services;
    using CsvHelper;
    using CsvHelper.Configuration;
    using System.Globalization;
    using ExcelDataReader;
    using System.Data;
    using Microsoft.Extensions.Caching.Memory;

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
            Task<List<UiAssignmentBlockDto>> GetStructuredFieldMetadata();
            void ClearAllCaches();
    }

        /// <summary>
        /// Service implementation for Field Metadata business logic
        /// </summary>
        public class FieldMetadataService : IFieldMetadataService
        {
            private readonly IFieldMetadataRepository _repository;
            private readonly ILogger<FieldMetadataService> _logger;
            private readonly IMemoryCache _memoryCache;
            private const string CacheKeyAllWithValues = "field_metadata_with_values";
            private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

            public FieldMetadataService(IFieldMetadataRepository repository, ILogger<FieldMetadataService> logger, IMemoryCache memoryCache)
            {
                _repository = repository ?? throw new ArgumentNullException(nameof(repository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            }
        public void ClearAllCaches()
        {
            _memoryCache.Remove(CacheKeyAllWithValues);

            // If you have more cache keys, remove them here
        }
        public async Task<List<UiAssignmentBlockDto>> GetStructuredFieldMetadata()
        {
            _logger.LogInformation("Getting structured field metadata");

            var fields = await GetAllWithValuesAsync();

            var structured = fields
                .GroupBy(f => string.IsNullOrWhiteSpace(f.UIAssignmentBlock)
                                ? "Others"
                                : f.UIAssignmentBlock)
                .Select(block => new UiAssignmentBlockDto
                {
                    UiAssignmentBlock = block.Key,
                    Subjects = block
                        .GroupBy(f => string.IsNullOrWhiteSpace(f.Subject)
                                        ? "Others"
                                        : f.Subject)
                        .OrderBy(g => g.Key == "Others")
                        .ThenBy(g => g.Key)
                        .Select(subjectGroup => new SubjectDto
                        {
                            Subject = subjectGroup.Key,
                            Fields = subjectGroup
                                        .OrderBy(f => f.FieldName)
                                        .ToList()
                        })
                        .ToList()
                })
                .OrderBy(b => b.UiAssignmentBlock == "Others")
                .ThenBy(b => b.UiAssignmentBlock)
                .ToList();

            _logger.LogInformation("Structured metadata generated successfully.");

            return structured;
        }
        public async Task<PagedResponse<FieldMetadataDto>> GetAllAsync(FieldMetadataQueryDto query)
            {
                _logger.LogInformation("Getting all field metadata with filters");

                var items = await _repository.GetAllAsync(
                    query.FieldName,
                    query.UIAssignmentBlock,
                    query.DataType,
                    query.PageNumber,
                    query.PageSize
                );

                var totalCount = await _repository.GetTotalCountAsync(
                    query.FieldName,
                    query.UIAssignmentBlock,
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
                    UIAssignmentBlock = createDto.UIAssignmentBlock,
                    Subject = createDto.Subject,
                    IsActive = createDto.IsActive,
                    CreatedDate = DateTime.UtcNow
                    // ValidationType, IsMandatory, UIControlType are computed - not set here
                };

                await _repository.CreateAsync(entity);

                // Invalidate cache
                InvalidateCache();

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
                existing.UIAssignmentBlock = updateDto.UIAssignmentBlock;
                existing.IsActive = updateDto.IsActive;
                existing.Subject= updateDto.Subject;

                var rowsAffected = await _repository.UpdateAsync(fieldName, existing);
            
                // Invalidate cache on successful update
                if (rowsAffected > 0)
                {
                    InvalidateCache();
                }
            
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
            
                // Invalidate cache on successful delete
                if (rowsAffected > 0)
                {
                    InvalidateCache();
                }
            
                return rowsAffected > 0;
            }

            private void InvalidateCache()
            {
                _logger.LogInformation("Invalidating field metadata cache");
                _memoryCache.Remove(CacheKeyAllWithValues);
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
                    UIAssignmentBlock = entity.UIAssignmentBlock,
                    Subject = entity.Subject,
                    UIControlType = entity.UIControlType,
                    IsActive = entity.IsActive,
                    CreatedDate = entity.CreatedDate
                };
            }

            public async Task<List<FieldMetadataWithValuesDto>> GetAllWithValuesAsync()
            {
                _logger.LogInformation("Getting all field metadata with check table and passable values");

                // Try to get from cache first
                if (_memoryCache.TryGetValue(CacheKeyAllWithValues, out List<FieldMetadataWithValuesDto> cachedResult))
                {
                    _logger.LogInformation("Returning cached field metadata with values");
                    return cachedResult;
                }

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
                        UIAssignmentBlock = metadata.UIAssignmentBlock,
                        Subject = metadata.Subject,
                        UIControlType = metadata.UIControlType,
                        IsActive = metadata.IsActive,
                        CreatedDate = metadata.CreatedDate,
                        ModifiedDate = null, // Add this to FieldMetadata model if needed
                        CheckTableValues = checkTableValues.Count > 0
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
                        PassableValues = passableValues.Count > 0
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

                // Cache the result for 30 minutes
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheExpiration
                };
                _memoryCache.Set(CacheKeyAllWithValues, result, cacheOptions);

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
            var existingRecords = await _repository.GetAllAsync(null, null, null, 1, 100000);

            var existingRowSet = new HashSet<string>(
                existingRecords.Select(e =>
                    $"{e.FieldName}|{e.DataElement}|{e.Description}|{e.KeyField}|{e.CheckTable}|{e.DataType}|{e.FieldLength}|{e.Decimals}|{e.HasDropdown}|{e.UIAssignmentBlock}|{e.Subject}"
                ),
                StringComparer.OrdinalIgnoreCase
            );

            var fileRowSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                    var createDto = TransformCsvRowToDto(row, normalizedFieldName);
                    var rowKey = BuildRowKey(createDto);

                    // Duplicate inside uploaded file
                    if (fileRowSet.Contains(rowKey))
                    {
                        result.ImportStatus = "SKIPPED";
                        result.ErrorCode = "DUPLICATE_IN_FILE";
                        result.ErrorMessage = "Duplicate row in file ignored";
                        response.Skipped++;
                        row.Result = result;
                        continue;
                    }

                    // Duplicate already in database
                    if (existingRowSet.Contains(rowKey))
                    {
                        result.ImportStatus = "FAILED";
                        result.ErrorCode = "DUPLICATE_ROW";
                        result.ErrorMessage = "Full row already exists in database";
                        response.Failed++;
                        row.Result = result;
                        continue;
                    }

                  

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

                    fileRowSet.Add(rowKey);
                    existingRowSet.Add(rowKey);

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
                string? uiAssignmentBlock =string.IsNullOrWhiteSpace(row.UIAssignmentBlock?.Trim())? null: row.UIAssignmentBlock.Trim();
                string? dataElement = string.IsNullOrWhiteSpace(row.DataElement?.Trim()) ? null : row.DataElement.Trim();
                string? description = string.IsNullOrWhiteSpace(row.Description?.Trim()) ? null : row.Description.Trim();
                string? keyField = row.KeyField?.Trim().ToUpper() == "X" ? "X" : null;
                string? checkTable = string.IsNullOrWhiteSpace(row.Checktable?.Trim()) ? null : row.Checktable.Trim();
                string? dataType = row.Datatype?.Trim().ToUpper() ?? "";
                int? fieldLength = int.TryParse(row.Length, out var len) && len > 0 ? len : null;
                int? decimals = int.TryParse(row.Decimals, out var dec) && dec >= 0 ? dec : null;
                string? subject = string.IsNullOrWhiteSpace(row.Subject?.Trim())? null : row.Subject.Trim();

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
                    UIAssignmentBlock = uiAssignmentBlock,
                    Subject = subject
                };
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
                    UIAssignmentBlock = dto.UIAssignmentBlock,
                    Subject = dto.Subject,
                };
            }
        private string BuildRowKey(CreateFieldMetadataDto dto)
        {
            return $"{dto.FieldName}|{dto.DataElement}|{dto.Description}|{dto.KeyField}|{dto.CheckTable}|{dto.DataType}|{dto.FieldLength}|{dto.Decimals}|{dto.HasDropdown}|{dto.UIAssignmentBlock}|{dto.Subject}";
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
