using Newtonsoft.Json;

namespace Rapid.Function.Models;

public class CsvToJsonRequestBody
{
    [JsonProperty("fileContent", NullValueHandling = NullValueHandling.Ignore)]
    public string FileContent { get; set; } = string.Empty;

    [JsonProperty("delimiter", NullValueHandling = NullValueHandling.Ignore)]
    public string Delimiter { get; set; } = ",";

    [JsonProperty("hasColumnHeaders", NullValueHandling = NullValueHandling.Ignore)]
    public bool HasColumnHeaders { get; set; } = true;

    [JsonProperty("removeTopRows", NullValueHandling = NullValueHandling.Ignore)]
    public int RemoveTopRows { get; set; }

    [JsonProperty("removeBottomRows", NullValueHandling = NullValueHandling.Ignore)]
    public int RemoveBottomRows { get; set; }

    [JsonProperty("ignoreBlankLines", NullValueHandling = NullValueHandling.Ignore)]
    public bool IgnoreBlankLines { get; set; } = true;
}