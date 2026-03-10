using FieldMetadataAPI.Models;

namespace FieldMetadataAPI.Repositories
{
    public interface IMaterialTypeRepository
    {
        Task<IEnumerable<MaterialType>> GetAllAsync();
    }
}