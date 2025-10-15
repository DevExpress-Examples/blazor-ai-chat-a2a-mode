using A2A;
using A2A.AspNetCore;
using A2AServer.Agents;
using ResearcherAgentServer.Agents;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

TaskManager taskManager = new TaskManager();

var researcherAgent = new ResearcherAgent();
researcherAgent.Attach(taskManager);

app.UseHttpsRedirection();

app.MapA2A(taskManager, "/agent");

app.Run();
