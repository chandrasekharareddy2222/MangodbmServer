namespace FieldMetadataAPI.Models
{
    public class CheckTableResponse
    {

        public string? CheckTableName { get; set; }
        public string? KeyValue { get; set; }
        public string? Description { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public int? IsValid { get; set; }
        public string? Message { get; set; }
    }
}
