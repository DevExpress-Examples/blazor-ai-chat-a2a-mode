using A2A;
using Microsoft.Extensions.AI;

namespace A2AServer.Agents.Base {
    /// <summary>
    /// Abstract base class for agents supporting TaskManager attachment and chat client initialization.
    /// </summary>
    public abstract class BaseAgent {
        private IChatClient? _innerChatClient;
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAgent"/> class.
        /// </summary>
        /// <param name="taskManager">The task manager to attach to this agent.</param>
        /// <exception cref="ArgumentNullException">Thrown when taskManager is null.</exception>
        protected BaseAgent(IChatClient chatClient, ITaskManager taskManager) {
            InnerChatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
            TaskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        }

        /// <summary>
        /// Gets and sets the inner chat client used by this agent.
        /// </summary>
        protected IChatClient? InnerChatClient
        {
            get => _innerChatClient;
            set => _innerChatClient = value;
        }

        /// <summary>
        /// Gets the TaskManager used by this agent.
        /// </summary>
        protected ITaskManager TaskManager { get; }
    }
}

