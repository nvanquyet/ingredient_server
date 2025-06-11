using IngredientServer.Core.Interfaces.Services;
using OpenAI.Chat;
using Azure;
using Azure.AI.OpenAI;
namespace IngredientServer.Core.Services;

public class AIService : IAIService
{
    private readonly Uri endpoint = new Uri("https://nvanq-mbrhssqv-eastus2.cognitiveservices.azure.com/");
    private readonly String deploymentName = "gpt-4.1";
    private readonly String apiKey = "APIKEY";


    private readonly AzureOpenAIClient azureClient;
    private readonly ChatClient chatClient;

    public AIService()
    {
        azureClient = new(
            endpoint,
            new AzureKeyCredential(apiKey));
        chatClient = azureClient.GetChatClient(deploymentName);

        GetChatResponseAsync("hello");
    }

   
    public Task<string> GetChatResponseAsync(string prompt, List<ChatMessage> messages = null)
    {
        var requestOptions = new ChatCompletionOptions()
        {
            Temperature = 1.0f,
            TopP = 1.0f,
            FrequencyPenalty = 0.0f,
            PresencePenalty = 0.0f,
        };

        messages ??= new List<ChatMessage>()
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage("I am going to Paris, what should I see?"),
        };
        
        var response = chatClient.CompleteChat(messages, requestOptions);
        System.Console.WriteLine(response.Value.Content[0].Text);
        return Task.FromResult(response.Value.Content[0].Text);
    }
}


