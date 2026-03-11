namespace FieldMetadataAPI.DTOs
{
    /// <summary>
    /// DTO for Field Metadata response
    /// </summary>
    public class FieldMetadataDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string? DataElement { get; set; }
        public string? Description { get; set; }
        public string? KeyField { get; set; }
        public string? CheckTable { get; set; }
        public string? DataType { get; set; }
        public int? FieldLength { get; set; }
        public int? Decimals { get; set; }
        public string? ValidationType { get; set; }
        public string? HasDropdown { get; set; }
        public bool IsMandatory { get; set; }
        public string? UIAssignmentBlock { get; set; }
        public string? UIControlType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Subject { get; set; }

        public string? Coordinate { get; set; }
    }

    /// <summary>
    /// DTO for creating new Field Metadata
    /// </summary>
    public class CreateFieldMetadataDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string? DataElement { get; set; }
        public string? Description { get; set; }
        public string? KeyField { get; set; }
        public string? CheckTable { get; set; }
        public string? DataType { get; set; }
        public int? FieldLength { get; set; }
        public int? Decimals { get; set; }
        public string? HasDropdown { get; set; }
        public string? UIAssignmentBlock { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Subject { get; set; }
        // Note: ValidationType, IsMandatory, UIControlType are computed - not included
        public string? Coordinate { get; set; }
    }

    /// <summary>
    /// DTO for updating Field Metadata (only editable fields)
    /// </summary>
    public class UpdateFieldMetadataDto
    {
        public string? Description { get; set; }
        public string? CheckTable { get; set; }
        public string? HasDropdown { get; set; }
        public string? UIAssignmentBlock { get; set; }
        public bool IsActive { get; set; }
        public string? Subject { get; set; }
        public string? Coordinate { get; set; }
    }

    /// <summary>
    /// Paginated response
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Query parameters for filtering and pagination
    /// </summary>
    public class FieldMetadataQueryDto
    {
        public string? FieldName { get; set; }
        public string? UIAssignmentBlock { get; set; }
        public string? DataType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO for bulk updating IsMandatory field
    /// </summary>
    public class BulkUpdateMandatoryDto
    {
        public List<FieldMandatoryUpdate> Updates { get; set; } = new();
    }

    /// <summary>
    /// Individual field mandatory update
    /// </summary>
    public class FieldMandatoryUpdate
    {
        public string FieldName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
    }
}
