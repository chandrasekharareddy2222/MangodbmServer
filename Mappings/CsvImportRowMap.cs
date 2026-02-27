using CsvHelper.Configuration;
using FieldMetadataAPI.DTOs;

namespace FieldMetadataAPI.Mappings
{
    /// <summary>
    /// ClassMap for CSV import rows with result tracking
    /// </summary>
    public sealed class CsvImportRowMap : ClassMap<CsvImportRow>
    {
        public CsvImportRowMap()
        {
            Map(m => m.Field).Name("Field");
            Map(m => m.DataElement).Name("Data element").Optional();
            Map(m => m.Description).Name("Short Description").Optional();
            Map(m => m.KeyField).Name("Key Field").Optional();
            Map(m => m.Checktable).Name("Check Tabel").Optional();
            Map(m => m.Datatype).Name("Data Type").Optional();
            Map(m => m.Length).Name("Length").Optional();
            Map(m => m.Decimals).Name("Decimals").Optional();
            Map(m => m.PossibleValues).Name("Possible values").Optional();
            Map(m => m.Result).Ignore();
            Map(m => m.Subject).Name("Subject");
        }
    }
}
