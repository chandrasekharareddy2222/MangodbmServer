using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;

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
    }
}
