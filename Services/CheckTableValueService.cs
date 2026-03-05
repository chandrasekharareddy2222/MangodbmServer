using FieldMetadataAPI.Data;
using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;
using Microsoft.AspNetCore.Connections;

namespace FieldMetadataAPI.Services
{
    public interface ICheckTableValueService
    {
        Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName);
        Task<int> CreateAsync(CreateCheckTableValueDto dto);
        Task<bool> UpdateAsync(int id, UpdateCheckTableValueDto dto);
        Task<bool> DeleteAsync(int id);
        Task<(int inserted, int skipped)> ImportValuesAsync(string tableName, List<CreateCheckTableValueDto> dtos);

    }
    public class CheckTableValueService : ICheckTableValueService
    {
        private readonly ICheckTableValueRepository _repository;
        private readonly ILogger<CheckTableValueService> _logger;
        private readonly IFieldMetadataService _fieldMetadataService;
       
        public CheckTableValueService(
            ICheckTableValueRepository repository,
            ILogger<CheckTableValueService> logger,
            IFieldMetadataService fieldMetadataService)
        {

            _repository = repository;
            _logger = logger;
            _fieldMetadataService = fieldMetadataService ?? throw new ArgumentNullException(nameof(fieldMetadataService));
        }
        private static bool IsAnyRequiredEmpty(string? key, string? desc, string? addi)
        {
            // Change required rules here:
            // If you want only KeyValue required -> return string.IsNullOrWhiteSpace(key);
            return string.IsNullOrWhiteSpace(key)
                || string.IsNullOrWhiteSpace(desc)
                || string.IsNullOrWhiteSpace(addi);
        }

        public async Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName)
        {
            var values = await _repository.GetByTableNameAsync(tableName);

            return values.Select(v => new CheckTableValueDto
            {
                CheckTableId = v.CheckTableID,
                TableName = v.CheckTableName,
                KeyValue = v.KeyValue,
                Description = v.Description,
                AdditionalInfo = v.AdditionalInfo,
                IsActive = v.IsActive,
                ValidFrom = v.ValidFrom,
                ValidTo = v.ValidTo,
                CreatedDate = v.CreatedDate,
                CreatedBy = v.CreatedBy
            }).ToList();
        }
        public async Task<int> CreateAsync(CreateCheckTableValueDto dto)
        {
            var entity = new CheckTableValue
            {
                CheckTableName = dto.CheckTableName,
                KeyValue = dto.KeyValue,
                Description = dto.Description,
                AdditionalInfo = dto.AdditionalInfo?.ToString(),
                IsActive = dto.IsActive,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                CreatedBy = dto.CreatedBy
            };

            var id = await _repository.CreateAsync(entity);
            
            // Clear field metadata cache since check table values have been modified
            _fieldMetadataService.ClearAllCaches();
            _logger.LogInformation("Cleared field metadata cache after creating check table value {Id}", id);
            
            return id;
        }
        public async Task<bool> UpdateAsync(
                    int id,
                    UpdateCheckTableValueDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return false;
            existing.KeyValue = dto.KeyValue;
            existing.Description = dto.Description;
            existing.AdditionalInfo = dto.AdditionalInfo;
            existing.IsActive = dto.IsActive;
            existing.ValidFrom = dto.ValidFrom;
            existing.ValidTo = dto.ValidTo;

            var updated = await _repository.UpdateAsync(id, existing);
            
            if (updated)
            {
                // Clear field metadata cache since check table values have been modified
                _fieldMetadataService.ClearAllCaches();
                _logger.LogInformation("Cleared field metadata cache after updating check table value {Id}", id);
            }
            
            return updated;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null || !existing.IsActive)
                return false;

            var deleted = await _repository.SoftDeleteAsync(id);
            
            if (deleted)
            {
                // Clear field metadata cache since check table values have been modified
                _fieldMetadataService.ClearAllCaches();
                _logger.LogInformation("Cleared field metadata cache after deleting check table value {Id}", id);
            }
            
            return deleted;
        }
        public async Task<(int inserted, int skipped)> ImportValuesAsync(string tableName, List<CreateCheckTableValueDto> dtos)
        {
            int inserted = 0;
            int skipped = 0;

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.KeyValue))
                {
                    skipped++;
                    continue;
                }

                // Use your existing repository method
                await _repository.InsertFromUploadAsync(
                    tableName,
                    dto.KeyValue,
                    dto.Description,
                    dto.AdditionalInfo?.ToString(),
                    "FILE_IMPORT"
                );
                inserted++;
            }

            _fieldMetadataService.ClearAllCaches();
            return (inserted, skipped);
        }

    }

} 

