using FieldMetadataAPI.Data;
using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;
using Microsoft.AspNetCore.Connections;
using ClosedXML.Excel;

namespace FieldMetadataAPI.Services
{
    public interface ICheckTableValueService
    {
        Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName);
        Task<int> CreateAsync(CreateCheckTableValueDto dto);
        Task<bool> UpdateAsync(int id, UpdateCheckTableValueDto dto);
        Task<bool> DeleteAsync(int id);
        Task<(int inserted, int skipped)> UploadFileAsync(string tableName, IFormFile file);

    }
    public class CheckTableValueService : ICheckTableValueService
    {
        private readonly ICheckTableValueRepository _repository;
        private readonly ILogger<CheckTableValueService> _logger;
        private readonly IFieldMetadataService _fieldMetadataService;
       
        public CheckTableValueService(
            ICheckTableValueRepository repository,
            ILogger<CheckTableValueService> logger,
            IFieldMetadataService fieldMetadataService)
        {

            _repository = repository;
            _logger = logger;
            _fieldMetadataService = fieldMetadataService ?? throw new ArgumentNullException(nameof(fieldMetadataService));
        }
        private static bool IsAnyRequiredEmpty(string? key, string? desc, string? addi)
        {
            // Change required rules here:
            // If you want only KeyValue required -> return string.IsNullOrWhiteSpace(key);
            return string.IsNullOrWhiteSpace(key)
                || string.IsNullOrWhiteSpace(desc)
                || string.IsNullOrWhiteSpace(addi);
        }

        public async Task<List<CheckTableValueDto>> GetByTableNameAsync(string tableName)
        {
            var values = await _repository.GetByTableNameAsync(tableName);

            return values.Select(v => new CheckTableValueDto
            {
                CheckTableId = v.CheckTableID,
                TableName = v.CheckTableName,
                KeyValue = v.KeyValue,
                Description = v.Description,
                AdditionalInfo = v.AdditionalInfo,
                IsActive = v.IsActive,
                ValidFrom = v.ValidFrom,
                ValidTo = v.ValidTo,
                CreatedDate = v.CreatedDate,
                CreatedBy = v.CreatedBy
            }).ToList();
        }
        public async Task<int> CreateAsync(CreateCheckTableValueDto dto)
        {
            var entity = new CheckTableValue
            {
                CheckTableName = dto.CheckTableName,
                KeyValue = dto.KeyValue,
                Description = dto.Description,
                AdditionalInfo = dto.AdditionalInfo?.ToString(),
                IsActive = dto.IsActive,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                CreatedBy = dto.CreatedBy
            };

            var id = await _repository.CreateAsync(entity);
            
            // Clear field metadata cache since check table values have been modified
            _fieldMetadataService.ClearAllCaches();
            _logger.LogInformation("Cleared field metadata cache after creating check table value {Id}", id);
            
            return id;
        }
        public async Task<bool> UpdateAsync(
                    int id,
                    UpdateCheckTableValueDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return false;
            existing.KeyValue = dto.KeyValue;
            existing.Description = dto.Description;
            existing.AdditionalInfo = dto.AdditionalInfo;
            existing.IsActive = dto.IsActive;
            existing.ValidFrom = dto.ValidFrom;
            existing.ValidTo = dto.ValidTo;

            var updated = await _repository.UpdateAsync(id, existing);
            
            if (updated)
            {
                // Clear field metadata cache since check table values have been modified
                _fieldMetadataService.ClearAllCaches();
                _logger.LogInformation("Cleared field metadata cache after updating check table value {Id}", id);
            }
            
            return updated;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null || !existing.IsActive)
                return false;

            var deleted = await _repository.SoftDeleteAsync(id);
            
            if (deleted)
            {
                // Clear field metadata cache since check table values have been modified
                _fieldMetadataService.ClearAllCaches();
                _logger.LogInformation("Cleared field metadata cache after deleting check table value {Id}", id);
            }
            
            return deleted;
        }
        public async Task<(int inserted, int skipped)> UploadFileAsync(string tableName, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception("tableName is required");

            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            (int inserted, int skipped) result = ext switch
            {
                ".csv" => await UploadCsvInternal(tableName, file),
                ".xlsx" => await UploadXlsxInternal(tableName, file),
                _ => throw new Exception("Only .csv and .xlsx files are supported")
            };

            _fieldMetadataService.ClearAllCaches();
            _logger.LogInformation("Cleared cache after upload for {TableName}. Inserted={Inserted}, Skipped={Skipped}",
                tableName, result.inserted, result.skipped);

            return result;
        }

        private async Task<(int inserted, int skipped)> UploadCsvInternal(string tableName, IFormFile file)
        {
            int inserted = 0, skipped = 0;

            using var reader = new StreamReader(file.OpenReadStream());
            bool isHeader = true;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (isHeader) { isHeader = false; continue; }
                if (string.IsNullOrWhiteSpace(line)) { skipped++; continue; }

                var cols = line.Split(',');

                var key = cols.ElementAtOrDefault(0)?.Trim();
                var desc = cols.ElementAtOrDefault(1)?.Trim();
                var addi = cols.ElementAtOrDefault(2)?.Trim();

                // STRICT RULE: any required empty => skip row
                if (IsAnyRequiredEmpty(key, desc, addi))
                {
                    skipped++;
                    continue;
                }

                await _repository.InsertFromUploadAsync(tableName, key!, desc!, addi!, "CSV_UPLOAD");
                inserted++;
            }

            return (inserted, skipped);
        }

        private async Task<(int inserted, int skipped)> UploadXlsxInternal(string tableName, IFormFile file)
        {
            int inserted = 0, skipped = 0;

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheets.FirstOrDefault()
                     ?? throw new Exception("No worksheet found in Excel file");

            var used = ws.RangeUsed();
            if (used == null) return (0, 0);

            int firstDataRow = used.FirstRow().RowNumber() + 1; // skip header
            int lastRow = used.LastRow().RowNumber();

            for (int r = firstDataRow; r <= lastRow; r++)
            {
                var key = ws.Cell(r, 1).GetValue<string>()?.Trim();
                var desc = ws.Cell(r, 2).GetValue<string>()?.Trim();
                var addi = ws.Cell(r, 3).GetValue<string>()?.Trim();

                // STRICT RULE: any required empty => skip row
                if (IsAnyRequiredEmpty(key, desc, addi))
                {
                    skipped++;
                    continue;
                }

                await _repository.InsertFromUploadAsync(tableName, key!, desc!, addi!, "XLSX_UPLOAD");
                inserted++;
            }

            return (inserted, skipped);
        }

    }

} 

