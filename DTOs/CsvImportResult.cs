namespace FieldMetadataAPI.DTOs
{
    /// <summary>
    /// Result of a single CSV import row
    /// </summary>
    public class ImportRowResult
    {
        public string FieldName { get; set; } = string.Empty;
        public string ImportStatus { get; set; } = string.Empty; // SUCCESS | FAILED | SKIPPED
        public string ErrorCode { get; set; } = string.Empty; // DUPLICATE_FIELD, VALIDATION_ERROR, SYSTEM_ERROR, etc.
        public string ErrorMessage { get; set; } = string.Empty; // Human-readable message
    }

    /// <summary>
    /// Summary response for CSV import operation
    /// </summary>
    public class CsvImportResponse
    {
        public int TotalRecords { get; set; }
        public int Inserted { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public string ResultFileName { get; set; } = "field_metadata_import_result.csv";
        public byte[] ResultFileContent { get; set; } = Array.Empty<byte>();
        public List<ImportRowResult> RowResults { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enhanced CSV row with import tracking columns
    /// </summary>
    public class CsvImportRow
    {
        // Original fields
        public string Field { get; set; } = string.Empty;
        public string? DataElement { get; set; }
        public string? Description { get; set; }
        public string? KeyField { get; set; }
        public string? Checktable { get; set; }
        public string? Datatype { get; set; }
        public string? Length { get; set; }
        public string? Decimals { get; set; }
        public string? PossibleValues { get; set; }
        public string? Subject { get; set; }

        // Track result
        public ImportRowResult Result { get; set; } = new();
    }
}
