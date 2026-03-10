using Dapper;
using FieldMetadataAPI.Data;
using FieldMetadataAPI.Models;
using Microsoft.Data.SqlClient;
using Serilog.Parsing;
using System.Data;

namespace FieldMetadataAPI.Repositories
{
    /// <summary>
    /// Repository interface for Field Metadata operations
    /// </summary>
    public interface IFieldMetadataRepository
    {
        Task<IEnumerable<FieldMetadata>> GetAllAsync(string? fieldName = null, string? tableGroup = null, string? dataType = null, int pageNumber = 1, int pageSize = 10);
        Task<int> GetTotalCountAsync(string? fieldName = null, string? tableGroup = null, string? dataType = null);
        Task<FieldMetadata?> GetByIdAsync(string fieldName);
        Task<int> CreateAsync(FieldMetadata fieldMetadata);
        Task<int> UpdateAsync(string fieldName, FieldMetadata fieldMetadata);
        Task<int> SoftDeleteAsync(string fieldName);
        Task<bool> ExistsAsync(string fieldName);
        Task<Dictionary<string, (FieldMetadata Metadata, List<CheckTableValue> CheckTableValues, List<PassableValue> PassableValues)>> GetAllWithValuesAsync();
        Task<int> BulkUpdateMandatoryAsync(List<(string FieldName, bool IsMandatory)> updates);
        Task<List<string>> GetAllFieldNamesAsync();
        Task<IEnumerable<string>> GetActiveCheckTablesAsync();
        Task<List<FieldMetadata>> GetAllRecordsAsync();
        Task BulkInsertAsync(List<FieldMetadata> records);
    }

    /// <summary>
    /// Repository implementation for Field Metadata using Dapper
    /// </summary>
    public class FieldMetadataRepository : IFieldMetadataRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<FieldMetadataRepository> _logger;
        public async Task BulkInsertAsync(List<FieldMetadata> records)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var table = new DataTable();

            table.Columns.Add("FieldName", typeof(string));
            table.Columns.Add("DataElement", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("KeyField", typeof(string));
            table.Columns.Add("Coordinate", typeof(string));
            table.Columns.Add("CheckTable", typeof(string));
            table.Columns.Add("DataType", typeof(string));
            table.Columns.Add("FieldLength", typeof(int));
            table.Columns.Add("Decimals", typeof(int));
            table.Columns.Add("HasDropdown", typeof(string));
            table.Columns.Add("UIAssignmentBlock", typeof(string));
            table.Columns.Add("Subject", typeof(string));

            foreach (var r in records)
            {
                table.Rows.Add(
                    r.FieldName,
                    r.DataElement,
                    r.Description,
                    r.KeyField,
                    r.Coordinate,
                    r.CheckTable,
                    r.DataType,
                    r.FieldLength,
                    r.Decimals,
                    r.HasDropdown,
                    r.UIAssignmentBlock,
                    r.Subject
                );
            }

            using var bulk = new SqlBulkCopy(connection)
            {
                DestinationTableName = "Field_Metadata",
                BatchSize = 1000,
                BulkCopyTimeout = 0
            };

            // ⭐ Explicit column mapping
            bulk.ColumnMappings.Add("FieldName", "FieldName");
            bulk.ColumnMappings.Add("DataElement", "DataElement");
            bulk.ColumnMappings.Add("Description", "Description");
            bulk.ColumnMappings.Add("KeyField", "KeyField");
            bulk.ColumnMappings.Add("Coordinate", "Coordinate");
            bulk.ColumnMappings.Add("CheckTable", "CheckTable");
            bulk.ColumnMappings.Add("DataType", "DataType");
            bulk.ColumnMappings.Add("FieldLength", "FieldLength");
            bulk.ColumnMappings.Add("Decimals", "Decimals");
            bulk.ColumnMappings.Add("HasDropdown", "HasDropdown");
            bulk.ColumnMappings.Add("UIAssignmentBlock", "UIAssignmentBlock");
            bulk.ColumnMappings.Add("Subject", "Subject");

            await bulk.WriteToServerAsync(table);
        }
        public FieldMetadataRepository(IDbConnectionFactory connectionFactory, ILogger<FieldMetadataRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<FieldMetadata>> GetAllAsync(string? fieldName = null, string? UIAssignmentBlock = null, string? dataType = null, int pageNumber = 1, int pageSize = 10)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    FieldName,
                    DataElement,
                    Description,
                    KeyField,
                    Coordinate,
                    CheckTable,
                    DataType,
                    FieldLength,
                    Decimals,
                    ValidationType,
                    HasDropdown,
                    IsMandatory,
                    UIAssignmentBlock,
                    Subject, 
                    UIControlType,
                    IsActive,
                    CreatedDate
                FROM Field_Metadata
                WHERE IsActive = 1
                AND (@FieldName IS NULL OR FieldName LIKE '%' + @FieldName + '%')
                AND (@UIAssignmentBlock IS NULL OR UIAssignmentBlock = @UIAssignmentBlock)
                AND (@DataType IS NULL OR DataType = @DataType)
                ORDER BY FieldName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var offset = (pageNumber - 1) * pageSize;

            _logger.LogInformation("Fetching field metadata with filters: FieldName={FieldName}, UIAssignmentBlock={UIAssignmentBlock}, DataType={DataType}, Page={PageNumber}, PageSize={PageSize}", 
                fieldName, UIAssignmentBlock, dataType, pageNumber, pageSize);

            return await connection.QueryAsync<FieldMetadata>(sql, new 
            { 
                FieldName = fieldName,
                UIAssignmentBlock = UIAssignmentBlock, 
                DataType = dataType,
                Offset = offset,
                PageSize = pageSize
            });
        }

        public async Task<int> GetTotalCountAsync(string? fieldName = null, string? UIAssignmentBlock = null, string? dataType = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT COUNT(*)
                FROM Field_Metadata
                WHERE IsActive = 1
                AND (@FieldName IS NULL OR FieldName LIKE '%' + @FieldName + '%')
                AND (@UIAssignmentBlock IS NULL OR UIAssignmentBlock = @UIAssignmentBlock)
                AND (@DataType IS NULL OR DataType = @DataType)";

            return await connection.ExecuteScalarAsync<int>(sql, new 
            { 
                FieldName = fieldName,
                UIAssignmentBlock = UIAssignmentBlock, 
                DataType = dataType
            });
        }

        public async Task<FieldMetadata?> GetByIdAsync(string fieldName)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    FieldName,
                    DataElement,
                    Description,
                    KeyField,
                    Coordinate,
                    CheckTable,
                    DataType,
                    FieldLength,
                    Decimals,
                    ValidationType,
                    HasDropdown,
                    IsMandatory,
                    UIAssignmentBlock,
                    Subject,
                    UIControlType,
                    IsActive,
                    CreatedDate
                FROM Field_Metadata
                WHERE FieldName = @FieldName AND IsActive = 1";

            _logger.LogInformation("Fetching field metadata by FieldName: {FieldName}", fieldName);

            return await connection.QueryFirstOrDefaultAsync<FieldMetadata>(sql, new { FieldName = fieldName });
        }

        public async Task<int> CreateAsync(FieldMetadata fieldMetadata)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Do not insert computed columns: ValidationType, IsMandatory, UIControlType
            var sql = @"
                INSERT INTO Field_Metadata 
                (
                    FieldName,
                    DataElement,
                    Description,
                    KeyField,
                    Coordinate,
                    CheckTable,
                    DataType,
                    FieldLength,
                    Decimals,
                    HasDropdown,
                    UIAssignmentBlock,
                    Subject,  
                    IsActive,
                    CreatedDate
                )
                VALUES 
                (
                    @FieldName,
                    @DataElement,
                    @Description,
                    @KeyField,
                    @Coordinate,
                    @CheckTable,
                    @DataType,
                    @FieldLength,
                    @Decimals,
                    @HasDropdown,
                    @UIAssignmentBlock,
                    @Subject,
                    @IsActive,
                    @CreatedDate
                )";

            _logger.LogInformation("Creating new field metadata: {FieldName}", fieldMetadata.FieldName);

            return await connection.ExecuteAsync(sql, fieldMetadata);
        }

        public async Task<int> UpdateAsync(string fieldName, FieldMetadata fieldMetadata)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Only update editable fields
            var sql = @"
                UPDATE Field_Metadata
                SET 
                    Description = @Description,
                    CheckTable = @CheckTable,
                    HasDropdown = @HasDropdown,
                    UIAssignmentBlock = @UIAssignmentBlock,
                    IsActive = @IsActive
                WHERE FieldName = @FieldName";

            _logger.LogInformation("Updating field metadata: {FieldName}", fieldName);

            return await connection.ExecuteAsync(sql, new
            {
                FieldName = fieldName,
                Description = fieldMetadata.Description,
                CheckTable = fieldMetadata.CheckTable,
                HasDropdown = fieldMetadata.HasDropdown,
                UIAssignmentBlock = fieldMetadata.UIAssignmentBlock,
                IsActive = fieldMetadata.IsActive
            });
        }

        public async Task<int> SoftDeleteAsync(string fieldName)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                UPDATE Field_Metadata
                SET IsActive = 0
                WHERE FieldName = @FieldName";

            _logger.LogInformation("Soft deleting field metadata: {FieldName}", fieldName);

            return await connection.ExecuteAsync(sql, new { FieldName = fieldName });
        }

        public async Task<bool> ExistsAsync(string fieldName)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT COUNT(1)
                FROM Field_Metadata
                WHERE FieldName = @FieldName";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { FieldName = fieldName });
            return count > 0;
        }

        public async Task<Dictionary<string, (FieldMetadata Metadata, List<CheckTableValue> CheckTableValues, List<PassableValue> PassableValues)>> GetAllWithValuesAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            _logger.LogInformation("Executing stored procedure sp_GetAllFieldMetadataWithValues");

            // Execute stored procedure and read multiple result sets
            using (var multi = await connection.QueryMultipleAsync("sp_GetAllFieldMetadataWithValues", commandType: CommandType.StoredProcedure))
            {
                // Result Set 1: Field Metadata
                var metadata = (await multi.ReadAsync<FieldMetadata>())
    .GroupBy(m => m.FieldName)
    .ToDictionary(g => g.Key, g => g.First());

                // Result Set 2: Check Table Values
                var checkTableValues = (await multi.ReadAsync<CheckTableValue>())
                    .GroupBy(c => c.CheckTableName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Result Set 3: Passable Values
                var passableValues = (await multi.ReadAsync<PassableValue>())
                    .GroupBy(p => p.FieldName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Combine results efficiently
                var result = new Dictionary<string, (FieldMetadata Metadata, List<CheckTableValue> CheckTableValues, List<PassableValue> PassableValues)>();

                foreach (var fieldMetadata in metadata.Values)
                {
                    var checkValues = string.IsNullOrEmpty(fieldMetadata.CheckTable) || !checkTableValues.ContainsKey(fieldMetadata.CheckTable)
                        ? new List<CheckTableValue>()
                        : checkTableValues[fieldMetadata.CheckTable];

                    var passValues = !passableValues.ContainsKey(fieldMetadata.FieldName)
                        ? new List<PassableValue>()
                        : passableValues[fieldMetadata.FieldName];

                    result[fieldMetadata.FieldName] = (fieldMetadata, checkValues, passValues);
                }

                return result;
            }
        }

        public async Task<int> BulkUpdateMandatoryAsync(List<(string FieldName, bool IsMandatory)> updates)
        {
            using var connection = _connectionFactory.CreateConnection();

            var table = new DataTable();
            table.Columns.Add("FieldName", typeof(string));
            table.Columns.Add("IsMandatory", typeof(bool));

            foreach (var update in updates)
            {
                table.Rows.Add(update.FieldName, update.IsMandatory);
            }

            var parameters = new DynamicParameters();
            parameters.Add(
                "@Updates",
                table.AsTableValuedParameter("FieldMandatoryUpdateType")
            );

            var rowsAffected = await connection.ExecuteAsync(
                "sp_BulkUpdateMandatoryFields",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }
        public async Task<List<string>> GetAllFieldNamesAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT FieldName
                FROM Field_Metadata
                WHERE IsActive = 1
                ORDER BY FieldName";

            _logger.LogInformation("Fetching all field names for duplicate detection");

            var fieldNames = await connection.QueryAsync<string>(sql);
            return fieldNames.ToList();
        }
        public async Task<List<FieldMetadata>> GetAllRecordsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
        SELECT 
            FieldName,
            DataElement,
            Description,
            KeyField,
            Coordinate, 
            CheckTable,
            DataType,
            FieldLength,
            Decimals,
            ValidationType,
            HasDropdown,
            IsMandatory,
            UIAssignmentBlock,
            Subject,
            UIControlType,
            IsActive,
            CreatedDate
        FROM Field_Metadata
        WHERE IsActive = 1";

            _logger.LogInformation("Fetching all field metadata records for duplicate row validation");

            var records = await connection.QueryAsync<FieldMetadata>(sql);
            return records.ToList();
        }

        public async Task<IEnumerable<string>> GetActiveCheckTablesAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            _logger.LogInformation("Fetching active check tables");

            return await connection.QueryAsync<string>(
                "sp_GetActiveCheckTablesOnly",
                commandType: CommandType.StoredProcedure);
        }
    }
}
