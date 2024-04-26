namespace Rapid.Core.Models
{
    internal class StandardizedRequestResultData
    {
        public string? RequestId { get; set; } = string.Empty;
        public string? RequestName { get; set; } = string.Empty;
        public string? SnapshotStartTime { get; set; } = string.Empty;
        public string? SnapshotTimeZone { get; set; } = string.Empty;
        public string? Identifier { get; set; } = string.Empty;
        public string? RC { get; set; } = string.Empty;
        public string? Date { get; set; } = string.Empty;
        public string FieldMnemonic { get; set; } = string.Empty;
        public string? Value { get; set; } = string.Empty;
    }
}