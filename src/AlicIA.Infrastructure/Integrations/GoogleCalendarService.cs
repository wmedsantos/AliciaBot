using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AlicIA.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AlicIA.Infrastructure.Integrations;

public class GoogleCalendarService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleCalendarService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var clientId = _configuration["Google:ClientId"];
        var clientSecret = _configuration["Google:ClientSecret"];

        var response = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            }),
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to refresh Google access token: {content}");

        using var doc = JsonDocument.Parse(content);

        var accessToken = doc.RootElement.GetProperty("access_token").GetString();

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("Google did not return an access token.");

        return accessToken;
    }

    public async Task<List<BusySlot>> GetBusySlotsAsync(
        CalendarConnection connection,
        DateTime timeMinUtc,
        DateTime timeMaxUtc,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await RefreshAccessTokenAsync(connection.RefreshToken, cancellationToken);

        var requestBody = new
        {
            timeMin = timeMinUtc.ToString("O"),
            timeMax = timeMaxUtc.ToString("O"),
            items = new[]
            {
                new { id = string.IsNullOrWhiteSpace(connection.CalendarId) ? "primary" : connection.CalendarId }
            }
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/calendar/v3/freeBusy");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to query Google FreeBusy API: {content}");

        using var doc = JsonDocument.Parse(content);

        var calendarKey = string.IsNullOrWhiteSpace(connection.CalendarId) ? "primary" : connection.CalendarId;

        if (!doc.RootElement.TryGetProperty("calendars", out var calendarsElement))
            return new List<BusySlot>();

        if (!calendarsElement.TryGetProperty(calendarKey, out var calendarElement))
        {
            // fallback defensivo: pega o primeiro calendário retornado
            var firstCalendar = calendarsElement.EnumerateObject().FirstOrDefault();
            if (firstCalendar.Equals(default(JsonProperty)))
                return new List<BusySlot>();

            calendarElement = firstCalendar.Value;
        }

        if (!calendarElement.TryGetProperty("busy", out var busyElement))
            return new List<BusySlot>();

        var result = new List<BusySlot>();

        foreach (var item in busyElement.EnumerateArray())
        {
            var start = item.GetProperty("start").GetDateTime();
            var end = item.GetProperty("end").GetDateTime();

            result.Add(new BusySlot(start, end));
        }

        return result;
    }

    public async Task<string> CreateEventAsync(
        CalendarConnection connection,
        string summary,
        DateTime startUtc,
        DateTime endUtc,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await RefreshAccessTokenAsync(connection.RefreshToken, cancellationToken);

        var requestBody = new
        {
            summary,
            description,
            start = new
            {
                dateTime = startUtc.ToString("O"),
                timeZone = "UTC"
            },
            end = new
            {
                dateTime = endUtc.ToString("O"),
                timeZone = "UTC"
            }
        };

        var requestJson = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(connection.CalendarId)}/events");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to create Google Calendar event: {content}");

        using var doc = JsonDocument.Parse(content);
        var eventId = doc.RootElement.GetProperty("id").GetString();

        if (string.IsNullOrWhiteSpace(eventId))
            throw new InvalidOperationException("Google Calendar did not return an event id.");

        return eventId;
    }
}

public record BusySlot(DateTime StartUtc, DateTime EndUtc);