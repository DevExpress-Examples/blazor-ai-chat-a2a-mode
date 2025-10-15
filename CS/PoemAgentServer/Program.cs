using A2A;
using A2A.AspNetCore;
using Azure;
using Azure.AI.OpenAI;
using A2AAgents.Shared;
using Microsoft.Extensions.AI;
using PoemAgentServer.Agents;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

TaskManager taskManager = new TaskManager();

var openAiServiceSettings = builder.Configuration.GetSection("AzureOpenAISettings").Get<AzureOpenAIServiceSettings>();
if(openAiServiceSettings == null ||
    string.IsNullOrEmpty(openAiServiceSettings.Endpoint) || openAiServiceSettings.Endpoint == "YOUR_END_POINT" ||
    string.IsNullOrEmpty(openAiServiceSettings.Key) || openAiServiceSettings.Key == "YOUR_API_KEY" ||
    string.IsNullOrEmpty(openAiServiceSettings.DeploymentName))
    throw new InvalidOperationException(
        "Specify the Azure OpenAI endpoint, key, and deployment name in the 'appsettings.json' file.");
var chatClient = new AzureOpenAIClient(
        new Uri(openAiServiceSettings.Endpoint),
        new AzureKeyCredential(openAiServiceSettings.Key))
    .GetChatClient(openAiServiceSettings.DeploymentName)
    .AsIChatClient();

var poemAgent = new PoemAgent(chatClient, taskManager);

app.UseHttpsRedirection();

app.MapA2A(taskManager, "/agent");

app.Run();
