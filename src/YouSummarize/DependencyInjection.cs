using Microsoft.Extensions.AI;
using YouSummarize.Managers;
using YouSummarize.Managers.AiGenerators;

namespace YouSummarize;

public static class DependencyInjection
{
    public static void AddManagers(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IAiManager, AiManager>();
        services.AddScoped<ErrorManager>();

        var openAiSecret = config["OPENAI_SECRET"];
        var deepSeekSecret = config["DEEPSEEK_SECRET"];

        ArgumentNullException.ThrowIfNull(openAiSecret);
        ArgumentNullException.ThrowIfNull(deepSeekSecret);

        services.AddKeyedChatClient("OpenAi", new OpenAI.Chat.ChatClient("gpt-4o-mini", openAiSecret).AsIChatClient());

        //IChatClient client = new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.1")

        //services.AddKeyedChatClient("DeepSeek", new OpenAIClientExtensions(new ApiKeyCredential(deepSeekSecret), new OpenAIClientOptions() { Endpoint = new Uri("https://api.deepseek.com") }).AsIChatClient());
    }
}
