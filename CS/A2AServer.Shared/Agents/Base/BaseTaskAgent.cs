using A2A;
using Microsoft.Extensions.AI;

namespace A2AServer.Agents.Base {
    /// <summary>
    /// Abstract agent for tasks, inherits base functionality.
    /// </summary>
    public abstract class BaseTaskAgent : BaseAgent {    
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTaskAgent"/> class.
        /// </summary>
        /// <param name="taskManager">The task manager to attach to this agent.</param>
        /// <exception cref="ArgumentNullException">Thrown when taskManager is null.</exception>
        public BaseTaskAgent(IChatClient chatClient, ITaskManager taskManager) : base(chatClient, taskManager) {
            
        }

        /// <summary>
        /// Handles the creation of a new task.
        /// </summary>
        protected abstract Task OnTaskCreated(AgentTask task, CancellationToken token);
        /// <summary>
        /// Handles updates to an existing task.
        /// </summary>
        protected abstract Task OnTaskUpdated(AgentTask task, CancellationToken token);
        /// <summary>
        /// Returns the agent card describing this agent's capabilities and skills for A2A discovery.
        /// </summary>
        protected abstract Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken);
    }
}

