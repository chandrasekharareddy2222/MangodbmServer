using Dapper;
using FieldMetadataAPI.Data;
using FieldMetadataAPI.Models;
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
    }

    /// <summary>
    /// Repository implementation for Field Metadata using Dapper
    /// </summary>
    public class FieldMetadataRepository : IFieldMetadataRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<FieldMetadataRepository> _logger;

        public FieldMetadataRepository(IDbConnectionFactory connectionFactory, ILogger<FieldMetadataRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<FieldMetadata>> GetAllAsync(string? fieldName = null, string? tableGroup = null, string? dataType = null, int pageNumber = 1, int pageSize = 10)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    FieldName,
                    DataElement,
                    Description,
                    KeyField,
                    CheckTable,
                    DataType,
                    FieldLength,
                    Decimals,
                    ValidationType,
                    HasDropdown,
                    IsMandatory,
                    TableGroup,
                    UIControlType,
                    IsActive,
                    CreatedDate
                FROM Field_Metadata
                WHERE IsActive = 1
                AND (@FieldName IS NULL OR FieldName LIKE '%' + @FieldName + '%')
                AND (@TableGroup IS NULL OR TableGroup = @TableGroup)
                AND (@DataType IS NULL OR DataType = @DataType)
                ORDER BY FieldName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var offset = (pageNumber - 1) * pageSize;

            _logger.LogInformation("Fetching field metadata with filters: FieldName={FieldName}, TableGroup={TableGroup}, DataType={DataType}, Page={PageNumber}, PageSize={PageSize}", 
                fieldName, tableGroup, dataType, pageNumber, pageSize);

            return await connection.QueryAsync<FieldMetadata>(sql, new 
            { 
                FieldName = fieldName, 
                TableGroup = tableGroup, 
                DataType = dataType,
                Offset = offset,
                PageSize = pageSize
            });
        }

        public async Task<int> GetTotalCountAsync(string? fieldName = null, string? tableGroup = null, string? dataType = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT COUNT(*)
                FROM Field_Metadata
                WHERE IsActive = 1
                AND (@FieldName IS NULL OR FieldName LIKE '%' + @FieldName + '%')
                AND (@TableGroup IS NULL OR TableGroup = @TableGroup)
                AND (@DataType IS NULL OR DataType = @DataType)";

            return await connection.ExecuteScalarAsync<int>(sql, new 
            { 
                FieldName = fieldName, 
                TableGroup = tableGroup, 
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
                    CheckTable,
                    DataType,
                    FieldLength,
                    Decimals,
                    ValidationType,
                    HasDropdown,
                    IsMandatory,
                    TableGroup,
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
                    CheckTable,
                    DataType,
                    FieldLength,
                    Decimals,
                    HasDropdown,
                    TableGroup,
                    IsActive,
                    CreatedDate
                )
                VALUES 
                (
                    @FieldName,
                    @DataElement,
                    @Description,
                    @KeyField,
                    @CheckTable,
                    @DataType,
                    @FieldLength,
                    @Decimals,
                    @HasDropdown,
                    @TableGroup,
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
                    TableGroup = @TableGroup,
                    IsActive = @IsActive
                WHERE FieldName = @FieldName";

            _logger.LogInformation("Updating field metadata: {FieldName}", fieldName);

            return await connection.ExecuteAsync(sql, new
            {
                FieldName = fieldName,
                Description = fieldMetadata.Description,
                CheckTable = fieldMetadata.CheckTable,
                HasDropdown = fieldMetadata.HasDropdown,
                TableGroup = fieldMetadata.TableGroup,
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
            
            var sql = @"
                SELECT 
                    fm.FieldName,
                    fm.DataElement,
                    fm.Description,
                    fm.KeyField,
                    fm.CheckTable,
                    fm.DataType,
                    fm.FieldLength,
                    fm.Decimals,
                    fm.ValidationType,
                    fm.HasDropdown,
                    fm.IsMandatory,
                    fm.TableGroup,
                    fm.UIControlType,
                    fm.IsActive,
                    fm.CreatedDate,
                    -- Check Table Values
                    ctv.CheckTableID,
                    ctv.CheckTableName,
                    ctv.KeyValue,
                    ctv.Description,
                    ctv.AdditionalInfo,
                    ctv.IsActive,
                    ctv.ValidFrom,
                    ctv.ValidTo,
                    ctv.CreatedDate,
                    ctv.CreatedBy,
                    -- Passable Values
                    pv.PassableID,
                    pv.FieldName,
                    pv.KeyValue,
                    pv.DisplayValue,
                    pv.Description,
                    pv.DisplayOrder,
                    pv.IsDefault,
                    pv.IconClass,
                    pv.ColorCode,
                    pv.IsActive,
                    pv.CreatedDate
                FROM dbo.Field_Metadata fm
                LEFT JOIN dbo.Check_Table_Values ctv 
                    ON fm.CheckTable = ctv.CheckTableName
                    AND ctv.IsActive = 1
                LEFT JOIN dbo.Passable_Values pv 
                    ON fm.FieldName = pv.FieldName
                    AND pv.IsActive = 1
                WHERE fm.IsActive = 1
                ORDER BY fm.FieldName, pv.DisplayOrder, ctv.KeyValue";

            _logger.LogInformation("Fetching all field metadata with check table and passable values");

            var result = new Dictionary<string, (FieldMetadata Metadata, List<CheckTableValue> CheckTableValues, List<PassableValue> PassableValues)>();

            await connection.QueryAsync<FieldMetadata, CheckTableValue, PassableValue, int>(
                sql,
                (metadata, checkTableValue, passableValue) =>
                {
                    if (!result.ContainsKey(metadata.FieldName))
                    {
                        result[metadata.FieldName] = (metadata, new List<CheckTableValue>(), new List<PassableValue>());
                    }

                    var entry = result[metadata.FieldName];

                    // Add check table value if not null and not already added
                    if (checkTableValue?.CheckTableID > 0 && 
                        !entry.CheckTableValues.Any(c => c.CheckTableID == checkTableValue.CheckTableID))
                    {
                        entry.CheckTableValues.Add(checkTableValue);
                    }

                    // Add passable value if not null and not already added
                    if (passableValue?.PassableID > 0 && 
                        !entry.PassableValues.Any(p => p.PassableID == passableValue.PassableID))
                    {
                        entry.PassableValues.Add(passableValue);
                    }

                    return 0; // Dummy return
                },
                splitOn: "CheckTableID,PassableID"
            );

            return result;
        }

        public async Task<int> BulkUpdateMandatoryAsync(List<(string FieldName, bool IsMandatory)> updates)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // IsMandatory is a computed column based on KeyField
            // If isMandatory = true, set KeyField = 'X'
            // If isMandatory = false, set KeyField = NULL
            
            var sql = @"
                UPDATE Field_Metadata
                SET KeyField = @KeyField
                WHERE FieldName = @FieldName
                AND IsActive = 1";

            _logger.LogInformation("Bulk updating IsMandatory for {Count} field(s)", updates.Count);

            var totalRowsAffected = 0;

            foreach (var (fieldName, isMandatory) in updates)
            {
                var keyFieldValue = isMandatory ? "X" : null;
                
                var rowsAffected = await connection.ExecuteAsync(sql, new 
                { 
                    KeyField = keyFieldValue,
                    FieldName = fieldName
                });

                totalRowsAffected += rowsAffected;
                
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Updated {FieldName}: IsMandatory = {IsMandatory}", fieldName, isMandatory);
                }
            }

            _logger.LogInformation("Successfully updated {RowsAffected} record(s)", totalRowsAffected);

            return totalRowsAffected;
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
    }
}
