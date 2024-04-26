using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Rapid.Core.Models;

namespace Rapid.Core
{
    public partial class Convert
    {
        public static async Task<ConversionResult> CsvToJson(CsvToJsonSettings settings)
        {
            try
            {
                var csvReaderConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    { Delimiter = settings.Delimiter, HasHeaderRecord = settings.HasColumnHeaders };

                Stream contentStream;
                if (IsBase64String(settings.FileContent))
                {
                    // File Content is base64 encoded
                    var contentBytes = System.Convert.FromBase64String(settings.FileContent);
                    contentStream = new MemoryStream(contentBytes);
                }
                else
                {
                    // File Content is not base64 encoded
                    var contentBytes = Encoding.UTF8.GetBytes(settings.FileContent);
                    contentStream = new MemoryStream(contentBytes);
                }

                using var reader = new StreamReader(contentStream);
                using var csv = new CsvReader(reader, csvReaderConfig);


                // Remove rows at the top.
                for (var i = 0; i < settings.RemoveTopRows; i++) await csv.ReadAsync();

                // Read all remaining rows.
                var records = csv.GetRecords<dynamic>().ToList();

                // Remove rows at the bottom.
                for (var i = 0; i < settings.RemoveBottomRows; i++) records.RemoveAt(records.Count - 1);

                var headerRead = false;
                var dataTable = new DataTable();
                foreach (var record in records)
                {
                    var row = dataTable.NewRow();
                    foreach (var item in record)
                    {
                        if (!headerRead) dataTable.Columns.Add(item.Key, typeof(string));
                        row[item.Key] = item.Value;
                    }

                    headerRead = true;
                    dataTable.Rows.Add(row);
                }

                // Serialize the object into a JSON string
                var jsonResult = JsonConvert.SerializeObject(dataTable, Formatting.Indented);

                // Return the standardized JSON string
                return new ConversionResult { Result = jsonResult, Message = "Success", IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ConversionResult { Message = ex.ToString(), IsSuccess = false };
            }
        }

        private static bool IsBase64String(string base64)
        {
            var buffer = new Span<byte>(new byte[base64.Length]);
            return System.Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}