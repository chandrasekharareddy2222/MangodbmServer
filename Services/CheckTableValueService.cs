using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Repositories;

namespace FieldMetadataAPI.Services
{
    public interface ICheckTableValueService
    {
        Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName);

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
    }  }
