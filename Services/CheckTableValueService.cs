using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;

namespace FieldMetadataAPI.Services
{
    public interface ICheckTableValueService
    {
        Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName);
        Task<int> CreateAsync(CreateCheckTableValueDto dto);
        Task<bool> UpdateAsync(int id, UpdateCheckTableValueDto dto);
        Task<bool> DeleteAsync(int id);

    }
    public class CheckTableValueService : ICheckTableValueService
    {
        private readonly ICheckTableValueRepository _repository;
        private readonly ILogger<CheckTableValueService> _logger;

        public CheckTableValueService(
            ICheckTableValueRepository repository,
            ILogger<CheckTableValueService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName)
        {
            var values = await _repository.GetByTableNameAsync(tableName);

            return values.Select(v => new CheckTableValueDto
            {
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

            return await _repository.CreateAsync(entity);
        }
        public async Task<bool> UpdateAsync(
                    int id,
                    UpdateCheckTableValueDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return false;

            existing.Description = dto.Description;
            existing.AdditionalInfo = dto.AdditionalInfo;
            existing.IsActive = dto.IsActive;
            existing.ValidFrom = dto.ValidFrom;
            existing.ValidTo = dto.ValidTo;

            return await _repository.UpdateAsync(id, existing);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null || !existing.IsActive)
                return false;

            return await _repository.SoftDeleteAsync(id);
        }

    } 
}
