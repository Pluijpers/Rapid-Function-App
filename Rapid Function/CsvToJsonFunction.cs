using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Rapid.Core.Models;
using Rapid.Function.Models;
using Convert = Rapid.Core.Convert;

namespace Rapid.Function;

public class CsvToJsonFunction
{
    private readonly ILogger<CsvToJsonFunction> _logger;

    public CsvToJsonFunction(ILogger<CsvToJsonFunction> log)
    {
        _logger = log;
    }

    [FunctionName("Csv To Json")]
    [OpenApiOperation("CsvToJson", Description = "Convert a CSV file into its JSON equivalent.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code",
        In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json; charset=utf-8", typeof(CsvToJsonRequestBody), Required = true,
        Description = "Request information with the file and conversion details.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json; charset=utf-8", typeof(string),
        Description = "OK with converted CSV file in JSON format.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain; charset=utf-8", typeof(string),
        Description = "BAD with error message.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req)
    {
        _logger.LogInformation("Csv To Json conversion function is triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var requestData = JsonConvert.DeserializeObject<CsvToJsonRequestBody>(requestBody);

            var settings = new CsvToJsonSettings
            {
                FileContent = requestData?.FileContent ?? string.Empty,
                Delimiter = requestData?.Delimiter ?? ",",
                HasColumnHeaders = requestData?.HasColumnHeaders ?? true,
                RemoveTopRows = requestData?.RemoveTopRows ?? 0,
                RemoveBottomRows = requestData?.RemoveBottomRows ?? 0,
                IgnoreBlankLines = requestData?.IgnoreBlankLines ?? true
            };

            if (string.IsNullOrEmpty(settings.FileContent))
            {
                const string message = "No value for the fileContent query string parameter provided.";

                _logger.LogError(message);

                var badResult = new BadRequestObjectResult(message);
                badResult.ContentTypes.Add("text/plain; charset=utf-8");
                badResult.StatusCode = (int)HttpStatusCode.BadRequest;
                return badResult;
            }

            var result = Convert.CsvToJson(settings);

            var okResult = new OkObjectResult(result.Result);
            okResult.ContentTypes.Add("application/json; charset=utf-8");
            okResult.StatusCode = (int)HttpStatusCode.OK;
            return okResult;
        }
        catch (Exception ex)
        {
            var badResult = new BadRequestObjectResult(ex.Message);
            badResult.ContentTypes.Add("text/plain; charset=utf-8");
            badResult.StatusCode = (int)HttpStatusCode.BadRequest;
            return badResult;
        }
    }
}