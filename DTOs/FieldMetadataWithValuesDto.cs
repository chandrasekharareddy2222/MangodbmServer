namespace FieldMetadataAPI.DTOs
{
    /// <summary>
    /// DTO for Check Table Values
    /// </summary>
    public class CheckTableValueDto
    {
        public int CheckTableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AdditionalInfo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// DTO for Passable Values
    /// </summary>
    public class PassableValueDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? DisplayValue { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
        public string? IconClass { get; set; }
        public string? ColorCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for Field Metadata with Check Table and Passable Values
    /// </summary>
    public class FieldMetadataWithValuesDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string? DataElement { get; set; }
        public string? Description { get; set; }
        public bool IsMandatory { get; set; }
        public string? CheckTable { get; set; }
        public string? DataType { get; set; }
        public int? FieldLength { get; set; }
        public int? Decimals { get; set; }
        public string? ValidationType { get; set; }
        public bool HasDropdown { get; set; }
        public string? UIAssignmentBlock { get; set; }
        public string? UIControlType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<CheckTableValueDto>? CheckTableValues { get; set; }
        public List<PassableValueDto>? PassableValues { get; set; }
        public string? Subject { get; set; }
    }
}
