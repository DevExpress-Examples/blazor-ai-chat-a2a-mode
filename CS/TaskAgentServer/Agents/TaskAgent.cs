using System.Diagnostics;
using A2A;
using A2AServer.Agents.Base;
using Microsoft.Extensions.AI;

namespace TaskAgentServer.Agents {
    /// <summary>
    /// A concrete task agent implementation that simulates task processing with status updates.
    /// </summary>
    public class TaskAgent : BaseTaskAgent {
        /// <summary>
        /// Activity source for tracking and tracing task agent operations.
        /// </summary>
        private static readonly ActivitySource ActivitySource = new("A2A.TaskAgent", "1.0.0");

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAgent"/> class.
        /// </summary>
        /// <param name="taskManager">The task manager to attach to this agent.</param>
        public TaskAgent(IChatClient chatClient, ITaskManager taskManager) : base(chatClient, taskManager)
        {
            Attach(taskManager);
        }
        
                
        /// <summary>
        /// Attaches the agent's event handlers to the task manager for task processing.
        /// </summary>
        /// <param name="taskManager">The task manager to attach handlers to.</param>
        public void Attach(ITaskManager taskManager) {
            taskManager.OnAgentCardQuery = GetAgentCardAsync;
            taskManager.OnTaskCreated = OnTaskCreated;
            taskManager.OnTaskUpdated = OnTaskUpdated;
        }

        /// <summary>
        /// Handles the creation of a new task by starting the update loop.
        /// </summary>
        /// <param name="task">The newly created task.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task OnTaskCreated(AgentTask task, CancellationToken cancellationToken) {
            await RunUpdateLoopAsync(task, cancellationToken);
        }

        /// <summary>
        /// Handles updates to an existing task by running the update loop.
        /// </summary>
        /// <param name="task">The task that was updated.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task OnTaskUpdated(AgentTask task, CancellationToken cancellationToken) {
            await RunUpdateLoopAsync(task, cancellationToken);
        }

        /// <summary>
        /// Simulates task processing by sending periodic status updates and completing the task.
        /// </summary>
        /// <param name="task">The task to process.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when TaskManager is not attached.</exception>
        private async Task RunUpdateLoopAsync(AgentTask task, CancellationToken cancellationToken) {
            var taskManager = TaskManager;
            if(taskManager == null)
                throw new InvalidOperationException("TaskManager is not attached.");

            using var activity = ActivitySource.StartActivity("RunUpdateLoop", ActivityKind.Server);
            activity?.SetTag("task.id", task.Id);

            for(int i = 0; i <= 5; i++) {
                if(cancellationToken.IsCancellationRequested)
                    break;
                await taskManager.UpdateStatusAsync(task.Id, TaskState.Working, new AgentMessage {
                    Parts = [new TextPart { Text = $"Update {i}/5" }]
                }, cancellationToken: cancellationToken);
                //simulate job
                await Task.Delay(1000, cancellationToken);
            }

            await taskManager.UpdateStatusAsync(task.Id, TaskState.Completed, new AgentMessage {
                Parts = [new TextPart { Text = "Task completed successfully" }]
            }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Returns the agent card describing this agent's capabilities and skills for A2A discovery.
        /// </summary>
        /// <param name="agentUrl">The URL where this agent can be reached.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An AgentCard describing this task agent's capabilities.</returns>
        protected async override Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested) {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var capabilities = new AgentCapabilities {
                Streaming = true,
                PushNotifications = false
            };
            return new AgentCard {
                Name = "Task Agent",
                Description = "Agent which sends 5 update messages every second.",
                Url = agentUrl,
                Version = "1.0.0",
                DefaultInputModes = ["text"],
                DefaultOutputModes = ["text"],
                Capabilities = capabilities,
                Skills = []
            };
        }
    }
}

