using CsvHelper.Configuration;
using FieldMetadataAPI.Models;

namespace FieldMetadataAPI.Mappings
{
    public class FieldMetadataMap : ClassMap<FieldMetadata>
    {
        public FieldMetadataMap()
        {
            Map(m => m.FieldName).Name("Field");
            Map(m => m.DataElement).Name("Data element").Optional();
            Map(m => m.Description).Name("Description").Optional();
            Map(m => m.KeyField).Name("Key Field").Optional();
            Map(m => m.CheckTable).Name("Checktable").Optional();
            Map(m => m.DataType).Name("Datatype");
            Map(m => m.FieldLength).Name("Length").Optional();
            Map(m => m.Decimals).Name("Decimals").Optional();
            Map(m => m.HasDropdown).Name("Possible values").Optional();
        }
    }
}