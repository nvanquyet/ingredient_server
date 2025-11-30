namespace IngredientServer.Core.Configuration;

/// <summary>
/// Configuration options for Azure OpenAI service
/// </summary>
public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    /// <summary>
    /// Azure OpenAI endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model name to use
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Maximum tokens for completion (default: 2000)
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// Request timeout in minutes (default: 2)
    /// </summary>
    public int TimeoutMinutes { get; set; } = 2;

    /// <summary>
    /// Temperature for text generation (default: 0.7)
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Temperature for image analysis (default: 0.3)
    /// </summary>
    public float ImageAnalysisTemperature { get; set; } = 0.3f;

    /// <summary>
    /// Frequency penalty (default: 0.1)
    /// </summary>
    public float FrequencyPenalty { get; set; } = 0.1f;

    /// <summary>
    /// Presence penalty (default: 0.1)
    /// </summary>
    public float PresencePenalty { get; set; } = 0.1f;

    /// <summary>
    /// Maximum concurrent requests (default: 10)
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 10;
}

