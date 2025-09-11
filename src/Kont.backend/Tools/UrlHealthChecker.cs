using Kont.backend.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kont.backend.Tools;
public class UrlHealthChecker : IHealthCheck
{
    private readonly ILogger<UrlHealthChecker> _logger;
    private readonly IHttpClientFactory _factory;
    private readonly Uri _requestUri;

    public UrlHealthChecker(ILogger<UrlHealthChecker> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing dependencies");

        using var client = _factory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(2);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = _requestUri,
        };

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return HealthCheckResult.Healthy("A healthy result.");
        }

        _logger.LogError("Dependency not available : " + response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogError(result);
        return HealthCheckResult.Unhealthy(result);
    }
}