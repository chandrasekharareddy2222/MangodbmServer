namespace FieldMetadataAPI.DTOs
{
    public class CheckTableValuesDto
    {
        public int CheckTableID { get; set; }
        public string? CheckTableName { get; set; }
        public string? KeyValue { get; set; }
        public string? Description { get; set; }
        public object? AdditionalInfo { get; set; }
        public bool? IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class CreateCheckTableValueDto
    {
        public string? CheckTableName { get; set; }
        public string? KeyValue { get; set; }
        public string? Description { get; set; }
        public object? AdditionalInfo { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class UpdateCheckTableValueDto
    {
        public string KeyValue { get; set; }
        public string? Description { get; set; }
        public string? AdditionalInfo { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
   

}
