namespace FieldMetadataAPI.Models
{
    /// <summary>
    /// Entity model for Check_Table_Values
    /// </summary>
    public class CheckTableValue
    {
        public int CheckTableID { get; set; }
        public string CheckTableName { get; set; } = string.Empty;
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
    /// Entity model for Passable_Values
    /// </summary>
    public class PassableValue
    {
        public int PassableID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public string? DisplayValue { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
        public string? IconClass { get; set; }
        public string? ColorCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
