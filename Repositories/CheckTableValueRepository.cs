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
        Task UploadExcelAsync(string tableName, IFormFile file);



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
        public async Task UploadExcelAsync(string tableName, IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rows = worksheet.Dimension.Rows;

            // FIX: Use the factory to ensure the connection is valid
            using var connection = _connectionFactory.CreateConnection();
            if (connection.State != ConnectionState.Open) connection.Open();

            for (int row = 2; row <= rows; row++)
            {
                var parameters = new
                {
                    CheckTableName = tableName,
                    KeyValue = worksheet.Cells[row, 1].Text,
                    Description = worksheet.Cells[row, 2].Text,
                    AdditionalInfo = worksheet.Cells[row, 3].Text
                };

                // Use Dapper to call the stored procedure consistently with your other methods
                await connection.ExecuteAsync(
                    "sp_InsertCheckTableValue",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }


    }   
}   
