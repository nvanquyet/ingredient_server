using OpenAI.Chat;

namespace IngredientServer.Core.Interfaces.Services;

public interface IAIService
{
    Task<string> GetChatResponseAsync(string prompt, List<ChatMessage> messages = null);
}