using Dapper;
using FieldMetadataAPI.Data;
using FieldMetadataAPI.Models;
using System.Data;

namespace FieldMetadataAPI.Repositories
{
    public interface ICheckTableValueRepository
    {
        Task<IEnumerable<CheckTableValue>> GetByTableNameAsync(string tableName);


    }

    public class CheckTableValueRepository : ICheckTableValueRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<CheckTableValueRepository> _logger;

        public CheckTableValueRepository(
            IDbConnectionFactory connectionFactory,
            ILogger<CheckTableValueRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<CheckTableValue>> GetByTableNameAsync(string tableName)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT *
                FROM Check_Table_Values
                WHERE CheckTableName = @TableName
                AND IsActive = 1
                ORDER BY KeyValue";

            return await connection.QueryAsync<CheckTableValue>(sql, new { TableName = tableName });
        }

        public async Task<CheckTableValue?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT *
                FROM Check_Table_Values
                WHERE CheckTableID = @Id";

            return await connection.QueryFirstOrDefaultAsync<CheckTableValue>(sql, new { Id = id });
        }
    }   }
