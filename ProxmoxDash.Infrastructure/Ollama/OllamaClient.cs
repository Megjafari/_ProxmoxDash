using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Ollama;

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClient(HttpClient httpClient, ILogger<OllamaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string prompt, string context)
    {
        var request = new
        {
            model = "llama3.1:8b",
            prompt = $"{context}\n\nUser: {prompt}",
            stream = false
        };

        var response = await _httpClient.PostAsync(
            "/api/generate",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        );

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return result?.Response ?? "No response";
    }
}

record OllamaResponse(string Response);