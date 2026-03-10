using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;

namespace FieldMetadataAPI.Services
{
    public class MaterialTypeService : IMaterialTypeService
    {
        private readonly IMaterialTypeRepository _repository;

        public MaterialTypeService(IMaterialTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MaterialType>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}