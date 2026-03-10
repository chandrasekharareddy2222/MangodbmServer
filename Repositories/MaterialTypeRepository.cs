using Dapper;
using FieldMetadataAPI.Data;
using FieldMetadataAPI.Models;
using System.Data;

namespace FieldMetadataAPI.Repositories
{
    public class MaterialTypeRepository : IMaterialTypeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MaterialTypeRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<MaterialType>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<MaterialType>(
                "sp_GetMaterialTypes",
                commandType: CommandType.StoredProcedure);
        }
    }
}