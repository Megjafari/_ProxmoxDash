using System.Net.Http.Json;
using ProxmoxDash.Core.Interfaces;

namespace ProxmoxDash.Infrastructure.Ollama;

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;

    public OllamaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ChatAsync(string prompt, string context)
    {
        var request = new OllamaRequest(
            Model: "llama3.1:8b",
            Prompt: $"{context}\n\nUser: {prompt}",
            Stream: false
        );

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return result?.Response ?? string.Empty;
    }
}

record OllamaRequest(string Model, string Prompt, bool Stream);
record OllamaResponse(string Response);