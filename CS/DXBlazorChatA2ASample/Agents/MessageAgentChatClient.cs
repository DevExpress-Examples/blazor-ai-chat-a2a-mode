using A2A;
using Microsoft.Extensions.AI;

namespace DXBlazorChatA2ASample.Agents {
    /// <summary>
    /// Implements IChatClient and delegates message processing to an A2A agent.
    /// </summary>
    public sealed class MessageAgentChatClient : DelegatingChatClient {
        /// <summary>
        /// The A2A client used for agent communication.
        /// </summary>
        private A2AClient _agentClient;
        
        /// <summary>
        /// The context ID for this chat session.
        /// </summary>
        private string _contextId;
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageAgentChatClient"/> class.
        /// </summary>
        /// <param name="innerClient">The inner chat client to use for fallback or additional processing.</param>
        /// <param name="agentClient">The A2A client for agent communication.</param>
        /// <param name="contextID">The context ID for this chat session.</param>
        public MessageAgentChatClient(IChatClient innerClient, A2AClient agentClient, string contextId) : base(innerClient) {
            if (innerClient == null)
                throw new ArgumentNullException(nameof(innerClient));
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
            return new AgentMessage() {
                Role = MessageRole.User,
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
            await foreach(var item in _agentClient.SendMessageStreamingAsync(msgSendParams, cancellationToken)) {
                if(item.Data is AgentMessage message) {
                    AgentMessage streamingResponse = message;
                    yield return new ChatResponseUpdate(ChatRole.Assistant, ((TextPart)streamingResponse.Parts[0]).Text);
                }
            }
        }
    }
}
