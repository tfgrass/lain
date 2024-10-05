using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;

class Program
{
    static async Task Main(string[] args)
    {
var endpoint = "http://localhost:1234";
var openaiClient = new OpenAIClient("api-key", new OpenAIClientOptions
{
    Endpoint = new Uri(endpoint),
});

var lmAgent = new OpenAIChatAgent(
    chatClient: openaiClient.GetChatClient("<does-not-matter>"),
    name: "assistant")
    .RegisterMessageConnector()
    .RegisterPrintMessage();

await lmAgent.SendAsync("Can you write a piece of C# code to calculate 100th of fibonacci?");
    }
}
