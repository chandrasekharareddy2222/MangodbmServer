namespace FieldMetadataAPI.Models
{
    /// <summary>
    /// Entity model for Field_Metadata table
    /// </summary>
    public class FieldMetadata
    {
        /// <summary>
        /// Primary Key - Field Name
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Data Element
        /// </summary>
        public string? DataElement { get; set; }

        /// <summary>
        /// Field Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Key Field Indicator (X = Mandatory)
        /// </summary>
        public string? KeyField { get; set; }

        /// <summary>
        /// Check Table Reference
        /// </summary>
        public string? CheckTable { get; set; }

        /// <summary>
        /// Data Type (CHAR, NUMC, DATS, etc.)
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// Field Length
        /// </summary>
        public int? FieldLength { get; set; }

        /// <summary>
        /// Decimal Places
        /// </summary>
        public int? Decimals { get; set; }

        /// <summary>
        /// Validation Type - COMPUTED (read-only)
        /// </summary>
        public string? ValidationType { get; set; }

        /// <summary>
        /// Has Dropdown Indicator
        /// </summary>
        public string? HasDropdown { get; set; }

        /// <summary>
        /// Is Mandatory - COMPUTED (read-only)
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Table Group
        /// </summary>
        public string? UIAssignmentBlock { get; set; }

        /// <summary>
        /// UI Control Type - COMPUTED (read-only)
        /// </summary>
        public string? UIControlType { get; set; }

        /// <summary>
        /// Active Status
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Created Date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Subject
        /// </summary>
        public string? Subject { get; set; }
    }
}
