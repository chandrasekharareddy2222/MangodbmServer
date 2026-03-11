namespace FieldMetadataAPI.DTOs
{
    public class UiAssignmentBlockDto
    {
        public string UiAssignmentBlock { get; set; }
        public List<SubjectDto> Subjects { get; set; }
    }

    public class SubjectDto
    {
        public string Subject { get; set; }
        public List<FieldMetadataWithValuesDto> Fields { get; set; }
    }
}