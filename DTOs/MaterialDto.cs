namespace FieldMetadataAPI.DTOs
{
    /// <summary>
    /// DTO for material submission (handles both create and update)
    /// If MATNR is null/empty, it will be auto-generated (CREATE)
    /// If MATNR is provided, it will update existing material (UPDATE)
    /// </summary>
    public class MaterialSubmissionDto
    {
        public string? MATNR { get; set; }  // Null/Empty for CREATE, Provided for UPDATE
        
        // Mandatory Core Fields (required for CREATE, optional for UPDATE)
        public string? MTART { get; set; }  // Material Type
        public string? MEINS { get; set; }  // Base Unit of Measure
        
        // Optional Core Fields
        public string? MBRSH { get; set; }  // Industry Sector
        public string? MATKL { get; set; }  // Material Group
        
        // Additional attributes (dynamic fields from Field_Metadata)
        public Dictionary<string, object>? Attributes { get; set; }
        
        public string? SubmittedBy { get; set; }
    }

    /// <summary>
    /// DTO for material response
    /// </summary>
    public class MaterialDto
    {
        public string MATNR { get; set; } = string.Empty;
        
        // Core Fields
        public string MTART { get; set; } = string.Empty;
        public string MEINS { get; set; } = string.Empty;
        public string? MBRSH { get; set; }
        public string? MATKL { get; set; }
        
        // Additional Attributes
        public Dictionary<string, object> Attributes { get; set; } = new();
        
        // Administrative Fields
        public DateTime? ERSDA { get; set; }
        public string? ERNAM { get; set; }
        public DateTime? LAEDA { get; set; }
        public string? AENAM { get; set; }
        
        // Status
        public string LVORM { get; set; } = " ";
        public string Status { get; set; } = "ACTIVE";
        
        // Audit
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Query parameters for material search
    /// </summary>
    public class MaterialQueryDto
    {
        public string? MATNR { get; set; }
        public string? MTART { get; set; }
        public string? MATKL { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
