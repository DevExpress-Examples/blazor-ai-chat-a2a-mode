// Program.cs - Entry point for the Blazor A2A Chat Sample
// This file configures the DI container, registers agents, and sets up A2A endpoints.

using A2A;
using DXBlazorChatA2ASample.Services;
using DXBlazorChatA2ASample.Components;
using Microsoft.Extensions.AI;
using Azure;
using Azure.AI.OpenAI;
using DXBlazorChatA2ASample.Agents;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor and DevExpress services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => { options.SizeMode = DevExpress.Blazor.SizeMode.Medium; });
builder.Services.AddMvc();
builder.Services.AddScoped<DxThemesService>();
builder.Services.AddHttpContextAccessor();

// Configure Azure OpenAI chat client
var openAiServiceSettings = builder.Configuration.GetSection("AzureOpenAISettings").Get<AzureOpenAIServiceSettings>();
if (openAiServiceSettings == null ||
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

// Register the chat client for basic communication
builder.Services.AddScoped<IChatClient>(x => chatClient);

// Register PoemAgent as a keyed singleton for DI
builder.Services.AddKeyedScoped<IChatClient>(AgentsEndpoints.PoemAgent, (provider, key) => {
    var a2aclient = new A2AClient(new Uri(AgentsEndpoints.PoemAgent));
    return new MessageAgentChatClient(chatClient, a2aclient, AgentsEndpoints.PoemAgent);
});
// Register ShakespeareanStyleAgent as a keyed singleton for DI
builder.Services.AddKeyedScoped<IChatClient>(AgentsEndpoints.ShakespeareanStyleAgent, (provider, key) => {
    var a2aclient = new A2AClient(new Uri(AgentsEndpoints.ShakespeareanStyleAgent));
    return new MessageAgentChatClient(chatClient, a2aclient, AgentsEndpoints.ShakespeareanStyleAgent);
});
// Register TaskAgent as a keyed singleton for DI
builder.Services.AddKeyedScoped<IChatClient>(AgentsEndpoints.TaskAgent, (provider, key) => {
    var a2aclient = new A2AClient(new Uri(AgentsEndpoints.TaskAgent));
    return new TaskAgentChatClient(chatClient, a2aclient, AgentsEndpoints.TaskAgent);
});

builder.Services.AddKeyedScoped<IChatClient>(AgentsEndpoints.ResearcherAgent, (provider, key) => {
    var a2aclient = new A2AClient(new Uri(AgentsEndpoints.ResearcherAgent));
    return new TaskAgentChatClient(chatClient, a2aclient, AgentsEndpoints.ResearcherAgent);
});

builder.Services.AddDevExpressAI();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();