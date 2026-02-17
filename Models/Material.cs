namespace FieldMetadataAPI.Models
{
    /// <summary>
    /// Entity model for Material_Master
    /// </summary>
    public class MaterialMaster
    {
        public string MATNR { get; set; } = string.Empty;
        
        // All fields are optional except MATNR
        public string? MTART { get; set; }  // Material Type
        public string? MEINS { get; set; }  // Base Unit of Measure
        public string? MBRSH { get; set; }  // Industry Sector
        public string? MATKL { get; set; }  // Material Group
        
        // Administrative Fields
        public DateTime? ERSDA { get; set; }  // Created On
        public string? ERNAM { get; set; }    // Created By
        public DateTime? LAEDA { get; set; }  // Last Changed Date
        public string? AENAM { get; set; }    // Changed By
        
        // Status Fields
        public string LVORM { get; set; } = " ";  // Deletion Flag
        public string Status { get; set; } = "ACTIVE";  // Internal status
        
        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Entity model for Material_Attributes
    /// </summary>
    public class MaterialAttribute
    {
        public long AttributeID { get; set; }
        public string MATNR { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
        public string? DataType { get; set; }
        public int? FieldLength { get; set; }
        public int? Decimals { get; set; }
        public DateTime LastModified { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
