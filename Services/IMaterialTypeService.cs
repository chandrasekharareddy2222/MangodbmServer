using FieldMetadataAPI.Models;

namespace FieldMetadataAPI.Services
{
    public interface IMaterialTypeService
    {
        Task<IEnumerable<MaterialType>> GetAllAsync();
    }
}