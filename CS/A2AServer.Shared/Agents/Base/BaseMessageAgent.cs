using A2A;
using Microsoft.Extensions.AI;

namespace A2AServer.Agents.Base {
    /// <summary>
    /// Abstract agent for message processing, inherits base functionality.
    /// </summary>
    public abstract class BaseMessageAgent : BaseAgent {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMessageAgent"/> class.
        /// </summary>
        /// <param name="taskManager">The task manager to attach to this agent.</param>
        /// <exception cref="ArgumentNullException">Thrown when taskManager is null.</exception>
        public BaseMessageAgent(IChatClient chatClient, ITaskManager taskManager) : base(chatClient, taskManager) {
        }
        
        /// <summary>
        /// Attaches the agent's event handlers to the task manager for message processing.
        /// </summary>
        /// <param name="taskManager">The task manager to attach handlers to.</param>


        /// <summary>
        /// Returns the agent card describing this agent's capabilities and skills for A2A discovery.
        /// </summary>
        public abstract Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken);
        /// <summary>
        /// Processes an incoming A2A message and returns a response message.
        /// </summary>
        public abstract Task<A2AResponse> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken ct);
    }
}

