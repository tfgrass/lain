using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LMSConnector
{
    private readonly string _apiUrl;
    private readonly string _modelName;
    private readonly int _maxTokens;
    private readonly double _temperature;
    private readonly HttpClient _httpClient;

    public LMSConnector(string apiUrl = "http://127.0.0.1:1234/v1/chat/completions", string modelName = "", int maxTokens = -1, double temperature = 0.7)
    {
        _apiUrl = apiUrl;
        _modelName = modelName;
        _maxTokens = maxTokens;
        _temperature = temperature;
        _httpClient = new HttpClient();
    }

    public async Task SendAsync(string systemMessage, string userMessage, Action<string> onContent, Action<string>? onError = null)
    {
//        Console.WriteLine("Starting SendAsync method");

        var headers = new { ContentType = "application/json" };

        var messages = new[]
        {
            new { role = "system", content = systemMessage },
            new { role = "user", content = userMessage }
        };

        var payload = new
        {
            model = _modelName,
            messages = messages,
            temperature = _temperature,
            max_tokens = _maxTokens,
            stream = true
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        Console.WriteLine("Payload to be sent: " + jsonPayload);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
  //          Console.WriteLine($"Received response with status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                string? line;
                var buffer = new StringBuilder();

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    buffer.Append(line);
                    if (line.StartsWith("data: "))
                    {
                        var jsonStr = line.Substring("data: ".Length);
                        if (jsonStr == "[DONE]")
                        {
                            onContent("\n");
                            Console.WriteLine("Streaming completed");
                            break;
                        }
                        else
                        {
                            try
                            {
                                var chunkData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                                if (chunkData.TryGetProperty("choices", out var choices) &&
                                    choices[0].TryGetProperty("delta", out var delta) &&
                                    delta.TryGetProperty("content", out var partialContent))
                                {
                                    var content = partialContent.GetString();
                                    if (content != null)
                                    {
    //                                    Console.WriteLine("Partial content generated: " + content);
                                        onContent(content);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error parsing chunk: " + jsonStr);
                                onError?.Invoke("Error parsing chunk: " + jsonStr);
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Received non-OK status: {response.StatusCode}");
                onError?.Invoke($"Non-OK response: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Request failed: " + ex.Message);
            onError?.Invoke("Request failed: " + ex.Message);
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var connector = new LMSConnector();
        await connector.SendAsync(
            systemMessage: "You are a helpful assistant.",
            userMessage: "Can you write a piece of C# code to calculate the 100th Fibonacci number?",
            onContent: content => Console.Write(content),
            onError: error => Console.WriteLine("Error: " + error)
        );
    }
}