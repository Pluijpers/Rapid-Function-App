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
using Convert = Rapid.Function.Core.Convert;

namespace Rapid.Function;

public class StandardizeJsonFunction(ILogger<StandardizeJsonFunction> log)
{
    [FunctionName("StandardizeJsonDataRequest")]
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
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
    {
        log.LogInformation("Standardize Json Data Request function is triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var result = Convert.StandardizeJsonResponse(requestBody);

            if (!result.IsSuccess)
            {
                var message = result.Message;

                log.LogError(message);

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
}