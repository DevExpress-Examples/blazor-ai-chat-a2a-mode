using A2A;
using Microsoft.Extensions.AI;

namespace DXBlazorChatA2ASample.Agents {
    /// <summary>
    /// Implements IChatClient and delegates task-based message processing to an A2A agent.
    /// </summary>
    public sealed class TaskAgentChatClient : DelegatingChatClient {
        private readonly A2AClient _agentClient;
        private readonly string _contextId;
        private string? _waitingTask;
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAgentChatClient"/> class.
        /// </summary>
        /// <param name="innerClient">The inner chat client to use for fallback or additional processing.</param>
        /// <param name="agentClient">The A2A client for agent communication.</param>
        /// <param name="contextID">The context ID for this chat session.</param>
        public TaskAgentChatClient(IChatClient innerClient, A2AClient agentClient, string contextId) : base(innerClient) {
            _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
            _contextId = $"{contextId}-{new Guid().ToString()}";
        }

        /// <summary>
        /// Sends a message to the A2A agent and returns the response as a ChatResponse.
        /// </summary>
        public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            CancellationToken cancellationToken = new CancellationToken()) {
            var responses = await GetStreamingResponseAsync(messages, options, cancellationToken).ToArrayAsync(cancellationToken);
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, string.Join("", responses.Select(x => x.Text))));
        }

        /// <summary>
        /// Creates an A2A message from the provided chat messages.
        /// </summary>
        private AgentMessage CreateA2AMessage(IEnumerable<ChatMessage> messages) {
            var waitingTask = _waitingTask;
            _waitingTask = null;
            return new AgentMessage() {
                Role = MessageRole.User,
                TaskId = waitingTask,
                ContextId = _contextId,
                Parts = [new TextPart() { Text = messages.Last().Text }]
            };
        }



        /// <summary>
        /// Streams responses from the A2A agent as they are received.
        /// </summary>
        public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = new CancellationToken()) {
            MessageSendParams msgSendParams = new MessageSendParams() { Message = CreateA2AMessage(messages) };
            bool shouldBreak = false;
            await foreach(var item in _agentClient.SendMessageStreamingAsync(msgSendParams, cancellationToken)) {
                AgentTaskStatus? agentTaskStatus = (item.Data as AgentTask)?.Status;
                if(item.Data is TaskStatusUpdateEvent taskStatusUpdateEvent) {
                    agentTaskStatus = taskStatusUpdateEvent.Status;
                    if(agentTaskStatus.Value.State == TaskState.Completed) {
                        shouldBreak = true;
                    } else if(agentTaskStatus.Value.State == TaskState.InputRequired) {
                        _waitingTask = (item.Data as TaskUpdateEvent)?.TaskId;
                        shouldBreak = true;
                    }
                }
                if(agentTaskStatus != null)
                    yield return new ChatResponseUpdate(
                        ChatRole.Assistant,
                        $"Status Message: '{agentTaskStatus.Value.Message?.Parts[0].AsTextPart().Text}', Current State: '{agentTaskStatus.Value.State}' {Environment.NewLine}"
                    );
                if(shouldBreak) break;
            }
            yield return new ChatResponseUpdate(ChatRole.Assistant, "End of Agent Response." + Environment.NewLine);
        }
    }
}