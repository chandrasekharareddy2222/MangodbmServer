namespace FieldMetadataAPI.Models
{
    public class MaterialType
    {
        public int MaterialId { get; set; }

        public string? MaterialName { get; set; }

        public string? MaterialDescription { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? SelectedViews { get; set; }
    }
}