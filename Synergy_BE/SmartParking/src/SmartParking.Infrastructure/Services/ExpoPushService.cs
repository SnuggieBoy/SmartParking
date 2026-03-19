using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.Infrastructure.Services;

public sealed class ExpoPushService : IExpoPushService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExpoPushService> _logger;

    private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

    public ExpoPushService(IHttpClientFactory httpClientFactory, ILogger<ExpoPushService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendPushAsync(IReadOnlyList<string> expoPushTokens, string title, string body, object? data = null, CancellationToken ct = default)
    {
        if (expoPushTokens == null || expoPushTokens.Count == 0) return;

        var validTokens = expoPushTokens
            .Where(t => !string.IsNullOrWhiteSpace(t) && (t.StartsWith("ExponentPushToken[") || t.StartsWith("https://exp.host/")))
            .ToList();

        if (validTokens.Count == 0)
        {
            _logger.LogWarning("Không có Expo Push Token hợp lệ để gửi");
            return;
        }

        var messages = validTokens.Select(token => new
        {
            to = token,
            title,
            body,
            data = data ?? new { },
            sound = "default"
        }).ToList();

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            var response = await client.PostAsJsonAsync(ExpoPushUrl, messages, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Expo Push API lỗi {StatusCode}: {Response}", response.StatusCode, errBody);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            if (result.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataEl.EnumerateArray())
                {
                    if (item.TryGetProperty("status", out var status) && status.GetString() == "error")
                    {
                        var errMsg = item.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown";
                        _logger.LogWarning("Expo Push ticket error: {Error}", errMsg);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gửi Push qua Expo: {Message}", ex.Message);
        }
    }
}
