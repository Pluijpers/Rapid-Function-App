using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Rapid.Function.Core.Models;
using Rapid.Function.Models;
using Convert = Rapid.Function.Core.Convert;

namespace Rapid.Function;

public class RapidFunction
{
    private readonly ILogger<RapidFunction> _logger;

    public RapidFunction(ILogger<RapidFunction> log)
    {
        _logger = log;
    }

    #region CSV to JSON

    [Function("CsvToJson")]
    [OpenApiOperation("CsvToJson", Description = "Convert a CSV file into its JSON equivalent.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json; charset=utf-8", typeof(CsvToJsonRequestBody), Required = true,
        Description = "Request information with the file and conversion details.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json; charset=utf-8", typeof(string),
        Description = "OK with converted CSV file in JSON format.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain; charset=utf-8", typeof(string),
        Description = "BAD with error message.")]
    public async Task<IActionResult> CsvToJson(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
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

    #endregion CSV to JSON

    #region Standardize JSON Data Request

    [Function("StandardizeJsonDataRequest")]
    [OpenApiOperation("StandardizeJsonDataRequest",
        Description =
            "Standardize Data License request data in JSON format regardless of the specified field mnemonics.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json; charset=utf-8", typeof(string), Required = true,
        Description = "Data License request data in JSON format.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json; charset=utf-8", typeof(string),
        Description = "OK with standardized request data in JSON format.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain; charset=utf-8", typeof(string),
        Description = "BAD with error message.")]
    public async Task<IActionResult> StandardizeJsonDataRequest(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest req)
    {
        _logger.LogInformation("Standardize Json Data Request function is triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var result = Rapid.Core.Convert.StandardizeJsonResponse(requestBody);

            if (!result.IsSuccess)
            {
                var message = result.Message;

                _logger.LogError(message);

                var badResult = new BadRequestObjectResult(message);
                badResult.ContentTypes.Add("text/plain; charset=utf-8");
                badResult.StatusCode = (int)HttpStatusCode.BadRequest;
                return badResult;
            }

            var jsonData = JsonConvert.SerializeObject(result.Result, Formatting.Indented);

            var okResult = new OkObjectResult(jsonData);
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

    #endregion
}