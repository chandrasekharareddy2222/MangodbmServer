using Dapper;
using FieldMetadataAPI.Data;
using FieldMetadataAPI.Models;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.Data;

namespace FieldMetadataAPI.Repositories
{
    public interface ICheckTableValueRepository
    {
        Task<IEnumerable<CheckTableValue>> GetByTableNameAsync(string tableName);
        Task<CheckTableValue?> GetByIdAsync(int id);
        Task<int> CreateAsync(CheckTableValue value);
        Task<bool> UpdateAsync(int id, CheckTableValue value);
        Task<bool> SoftDeleteAsync(int id);
        Task<int> InsertFromUploadAsync(string tableName, string keyValue, string? description, string? additionalInfo, string createdBy);



    }

    public class CheckTableValueRepository : ICheckTableValueRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<CheckTableValueRepository> _logger;
        private readonly string _connectionString;

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
                ORDER BY CreatedDate DESC";

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

        public async Task<int> CreateAsync(CheckTableValue value)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new
            {

                CheckTableName = value.CheckTableName,
                KeyValue = value.KeyValue,
                Description = value.Description,
                AdditionalInfo = value.AdditionalInfo,
                IsActive = value.IsActive,
                ValidFrom = value.ValidFrom,
                ValidTo = value.ValidTo,
                CreatedBy = value.CreatedBy ?? "SYSTEM"
            };

            var id = await connection.ExecuteScalarAsync<int>(
                "sp_InsertCheckTableValue",
                parameters,
                commandType: CommandType.StoredProcedure);

            return id;
        }
        public async Task<bool> UpdateAsync(int id, CheckTableValue value)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new
            {
                CheckTableID = id,
                value.KeyValue,
                value.Description,
                value.AdditionalInfo,
                value.IsActive,
                value.ValidFrom,
                value.ValidTo
            };

            var rows = await connection.ExecuteScalarAsync<int>(
                "sp_UpdateCheckTableValue",
                parameters,
                commandType: CommandType.StoredProcedure);

            return rows > 0;
        }
        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();

            var rows = await connection.ExecuteScalarAsync<int>(
                "sp_SoftDeleteCheckTableValue",
                new { CheckTableID = id },
                commandType: CommandType.StoredProcedure);

            return rows > 0;
        }
        public async Task<int> InsertFromUploadAsync(
      string tableName,
      string keyValue,
      string? description,
      string? additionalInfo,
      string createdBy)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new
            {
                CheckTableName = tableName,
                KeyValue = keyValue,
                Description = description,
                AdditionalInfo = additionalInfo,
                IsActive = true,
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Parse("9999-12-31"),
                CreatedBy = createdBy
            };

            // Make sure your sp_InsertCheckTableValue accepts these parameters.
            // If your SP sets defaults itself, still ok to pass them (as long as SP params exist).
            var id = await connection.ExecuteScalarAsync<int>(
                "sp_InsertCheckTableValue",
                parameters,
                commandType: CommandType.StoredProcedure);

            return id;
        }


    }   
}   
