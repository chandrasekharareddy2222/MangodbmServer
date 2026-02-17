using Dapper;
using FieldMetadataAPI.Data;
using FieldMetadataAPI.Models;
using System.Data;

namespace FieldMetadataAPI.Repositories
{
    /// <summary>
    /// Repository interface for Material operations
    /// </summary>
    public interface IMaterialRepository
    {
        Task<string> GenerateMatnrAsync();
        Task<string> CreateMaterialAsync(MaterialMaster material, List<MaterialAttribute> attributes);
        Task<bool> UpdateMaterialAsync(string matnr, MaterialMaster material, List<MaterialAttribute> attributes, string? modifiedBy);
        Task<MaterialMaster?> GetMaterialByIdAsync(string matnr);
        Task<List<MaterialAttribute>> GetMaterialAttributesAsync(string matnr);
        Task<(List<MaterialMaster> Materials, int TotalCount)> GetMaterialsAsync(string? matnr, string? mtart, string? matkl, string? status, int pageNumber, int pageSize);
        Task<bool> MaterialExistsAsync(string matnr);
    }

    /// <summary>
    /// Repository implementation for Material using Dapper
    /// </summary>
    public class MaterialRepository : IMaterialRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<MaterialRepository> _logger;

        public MaterialRepository(IDbConnectionFactory connectionFactory, ILogger<MaterialRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateMatnrAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT NEXT VALUE FOR dbo.SEQ_MATNR";
            
            var nextValue = await connection.ExecuteScalarAsync<long>(sql);
            var matnr = nextValue.ToString().PadLeft(18, '0');
            
            _logger.LogInformation("Generated new MATNR: {MATNR}", matnr);
            
            return matnr;
        }

        public async Task<string> CreateMaterialAsync(MaterialMaster material, List<MaterialAttribute> attributes)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // If MATNR is not provided, generate it
            if (string.IsNullOrWhiteSpace(material.MATNR))
            {
                material.MATNR = await GenerateMatnrAsync();
            }

            var masterSql = @"
                INSERT INTO Material_Master 
                (MATNR, MTART, MEINS, MBRSH, MATKL, ERSDA, ERNAM, LVORM, Status, CreatedDate)
                VALUES 
                (@MATNR, @MTART, @MEINS, @MBRSH, @MATKL, @ERSDA, @ERNAM, @LVORM, @Status, @CreatedDate)";

            await connection.ExecuteAsync(masterSql, new
            {
                material.MATNR,
                material.MTART,
                material.MEINS,
                material.MBRSH,
                material.MATKL,
                ERSDA = material.ERSDA ?? DateTime.Now,
                material.ERNAM,
                material.LVORM,
                material.Status,
                material.CreatedDate
            });

            // Insert attributes
            if (attributes != null && attributes.Any())
            {
                var attributeSql = @"
                    INSERT INTO Material_Attributes 
                    (MATNR, FieldName, FieldValue, DataType, FieldLength, Decimals, LastModified, ModifiedBy)
                    VALUES 
                    (@MATNR, @FieldName, @FieldValue, @DataType, @FieldLength, @Decimals, @LastModified, @ModifiedBy)";

                await connection.ExecuteAsync(attributeSql, attributes);
            }

            _logger.LogInformation("Created material {MATNR} with {Count} attributes", material.MATNR, attributes?.Count ?? 0);

            return material.MATNR;
        }

        public async Task<bool> UpdateMaterialAsync(string matnr, MaterialMaster material, List<MaterialAttribute> attributes, string? modifiedBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var masterSql = @"
                UPDATE Material_Master
                SET MTART = @MTART,
                    MEINS = @MEINS,
                    MBRSH = @MBRSH,
                    MATKL = @MATKL,
                    LAEDA = @LAEDA,
                    AENAM = @AENAM,
                    LVORM = @LVORM,
                    Status = @Status,
                    ModifiedDate = @ModifiedDate
                WHERE MATNR = @MATNR";

            var rowsAffected = await connection.ExecuteAsync(masterSql, new
            {
                MATNR = matnr,
                material.MTART,
                material.MEINS,
                material.MBRSH,
                material.MATKL,
                LAEDA = DateTime.Now,
                AENAM = modifiedBy,
                material.LVORM,
                material.Status,
                ModifiedDate = DateTime.Now
            });

            if (rowsAffected == 0)
                return false;

            // Delete existing attributes
            await connection.ExecuteAsync("DELETE FROM Material_Attributes WHERE MATNR = @MATNR", new { MATNR = matnr });

            // Insert new attributes
            if (attributes != null && attributes.Any())
            {
                var attributeSql = @"
                    INSERT INTO Material_Attributes 
                    (MATNR, FieldName, FieldValue, DataType, FieldLength, Decimals, LastModified, ModifiedBy)
                    VALUES 
                    (@MATNR, @FieldName, @FieldValue, @DataType, @FieldLength, @Decimals, @LastModified, @ModifiedBy)";

                await connection.ExecuteAsync(attributeSql, attributes);
            }

            _logger.LogInformation("Updated material {MATNR} with {Count} attributes", matnr, attributes?.Count ?? 0);

            return true;
        }

        public async Task<MaterialMaster?> GetMaterialByIdAsync(string matnr)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT MATNR, MTART, MEINS, MBRSH, MATKL, ERSDA, ERNAM, LAEDA, AENAM, 
                       LVORM, Status, CreatedDate, ModifiedDate
                FROM Material_Master
                WHERE MATNR = @MATNR";

            return await connection.QueryFirstOrDefaultAsync<MaterialMaster>(sql, new { MATNR = matnr });
        }

        public async Task<List<MaterialAttribute>> GetMaterialAttributesAsync(string matnr)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT AttributeID, MATNR, FieldName, FieldValue, DataType, FieldLength, 
                       Decimals, LastModified, ModifiedBy
                FROM Material_Attributes
                WHERE MATNR = @MATNR
                ORDER BY FieldName";

            var result = await connection.QueryAsync<MaterialAttribute>(sql, new { MATNR = matnr });
            return result.ToList();
        }

        public async Task<(List<MaterialMaster> Materials, int TotalCount)> GetMaterialsAsync(
            string? matnr, string? mtart, string? matkl, string? status, int pageNumber, int pageSize)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(matnr))
            {
                whereClauses.Add("MATNR LIKE '%' + @MATNR + '%'");
                parameters.Add("MATNR", matnr);
            }

            if (!string.IsNullOrEmpty(mtart))
            {
                whereClauses.Add("MTART = @MTART");
                parameters.Add("MTART", mtart);
            }

            if (!string.IsNullOrEmpty(matkl))
            {
                whereClauses.Add("MATKL = @MATKL");
                parameters.Add("MATKL", matkl);
            }

            if (!string.IsNullOrEmpty(status))
            {
                whereClauses.Add("Status = @Status");
                parameters.Add("Status", status);
            }

            var whereClause = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            var countSql = $"SELECT COUNT(*) FROM Material_Master {whereClause}";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            var offset = (pageNumber - 1) * pageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var sql = $@"
                SELECT MATNR, MTART, MEINS, MBRSH, MATKL, ERSDA, ERNAM, LAEDA, AENAM, 
                       LVORM, Status, CreatedDate, ModifiedDate
                FROM Material_Master
                {whereClause}
                ORDER BY CreatedDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var materials = await connection.QueryAsync<MaterialMaster>(sql, parameters);

            return (materials.ToList(), totalCount);
        }

        public async Task<bool> MaterialExistsAsync(string matnr)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT COUNT(1) FROM Material_Master WHERE MATNR = @MATNR";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { MATNR = matnr });
            
            return count > 0;
        }
    }
}
