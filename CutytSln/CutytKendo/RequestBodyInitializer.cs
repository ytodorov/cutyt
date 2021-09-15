
using Cutyt.Core.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace CutytKendoWeb;
public class RequestBodyInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public RequestBodyInitializer(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        RequestTelemetry requestTelemetry = telemetry as RequestTelemetry;
        if (requestTelemetry != null)
        {
            if (httpContextAccessor?.HttpContext?.Request != null)
            {
                var body = httpContextAccessor.HttpContext.Items["_custom_http_body"] as string;
                var decodedBody = HttpUtility.UrlDecode(body);
                if (!string.IsNullOrEmpty(body))
                {
                    requestTelemetry.Properties.Add("_custom_http_body", decodedBody);
                }
            }
        }
    }
}
