using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rapid.Function.Core.Models;

namespace Rapid.Function.Core
{
    public partial class Convert
    {
        /// <summary>
        /// Standardize Bloomberg's Data Request response in JSON for use by a SQL stored procedure as JSON parameter.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>Standardized JSON string</returns>
        public static ConversionResult StandardizeJsonResponse(string json)
        {
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<List<dynamic>>(json);
                if (deserializedObject == null)
                {
                    return new ConversionResult { Message = "Failed to deserialize JSON string.", IsSuccess = false };
                }

                // Parse the dynamic RequestData object into a standardized RequestData object
                var request = new List<List<RequestResultData>>();
                foreach (var data in deserializedObject)
                {
                    var requestData = new List<RequestResultData>();
                    foreach (var property in data)
                    {
                        var fieldMnemonic = property.Name;
                        var value = property.Value;

                        requestData.Add(new RequestResultData
                        {
                            FieldMnemonic = fieldMnemonic,
                            Value = value
                        });
                    }

                    request.Add(requestData);
                }

                if (request.Any() == false)
                {
                    return new ConversionResult { Message = "Failed to parse JSON string", IsSuccess = false };
                }

                // Process the Request Result Data object into Standardized Request Result Data object

                var result = new List<StandardizedRequestResultData>();
                foreach (var data in request)
                {
                    var requestId = data.FirstOrDefault(x => x.FieldMnemonic == "DL_REQUEST_ID");
                    var requestName = data.FirstOrDefault(x => x.FieldMnemonic == "DL_REQUEST_NAME");
                    var snapshotStartTime = data.FirstOrDefault(x => x.FieldMnemonic == "DL_SNAPSHOT_START_TIME");
                    var snapshotTimeZone = data.FirstOrDefault(x => x.FieldMnemonic == "DL_SNAPSHOT_TZ");
                    var identifier = data.FirstOrDefault(x => x.FieldMnemonic == "IDENTIFIER");
                    var rc = data.FirstOrDefault(x => x.FieldMnemonic == "RC");
                    var date = data.FirstOrDefault(x => x.FieldMnemonic == "DATE");
                    var lastUpdateDate = data.FirstOrDefault(x => x.FieldMnemonic == "LAST_UPDATE_DT");

                    foreach (var item in data)
                    {
                        if (item.FieldMnemonic.Equals("DL_REQUEST_ID", StringComparison.CurrentCultureIgnoreCase) ||
                            item.FieldMnemonic.Equals("DL_REQUEST_NAME", StringComparison.CurrentCultureIgnoreCase) ||
                            item.FieldMnemonic.Equals("DL_SNAPSHOT_START_TIME", StringComparison.CurrentCultureIgnoreCase) ||
                            item.FieldMnemonic.Equals("DL_SNAPSHOT_TZ", StringComparison.CurrentCultureIgnoreCase) ||
                            item.FieldMnemonic.Equals("IDENTIFIER", StringComparison.CurrentCultureIgnoreCase) ||
                            item.FieldMnemonic.Equals("RC", StringComparison.CurrentCultureIgnoreCase) ||
                            item.FieldMnemonic.Equals("DATE", StringComparison.CurrentCultureIgnoreCase)
                           )
                            continue;

                        var resultData = new StandardizedRequestResultData
                        {
                            RequestId = requestId?.Value,
                            RequestName = requestName?.Value,
                            SnapshotStartTime = snapshotStartTime?.Value,
                            SnapshotTimeZone = snapshotTimeZone?.Value,
                            Identifier = identifier?.Value,
                            RC = rc?.Value,
                            Date = string.IsNullOrEmpty(date?.Value) ? snapshotStartTime?.Value : date.Value,
                            FieldMnemonic = item.FieldMnemonic,
                            Value = item.Value
                        };

                        result.Add(resultData);
                    }
                }

                // Serialize the standardized RequestData object into a JSON string
                var jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented);

                // Return the standardized JSON string
                return new ConversionResult { Result = jsonResult, Message = "Success", IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ConversionResult { Message = ex.ToString(), IsSuccess = false };
            }
        }
    }
}
