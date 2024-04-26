namespace Rapid.Core.Models
{
    public class CsvToJsonSettings
    {
        public string FileContent { get; set; } = string.Empty;
        public string Delimiter { get; set; } = ",";
        public bool HasColumnHeaders { get; set; } = true;
        public int RemoveTopRows { get; set; } = 0;
        public int RemoveBottomRows { get; set; } = 0;
        public bool IgnoreBlankLines { get; set; } = true;
    }
}