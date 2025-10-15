using A2A;
using A2AServer.Agents.Base;
using Microsoft.Extensions.AI;

namespace ShakespeareanStyleAgentServer.Agents {
    /// <summary>
    /// An agent that transforms user input into Shakespearean-style text using AI chat capabilities.
    /// </summary>
    public class ShakespeareanStyleAgent : BaseMessageAgent {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShakespeareanStyleAgent"/> class.
        /// </summary>
        /// <param name="taskManager">The task manager to attach to this agent.</param>
        public ShakespeareanStyleAgent(IChatClient chatClient, ITaskManager taskManager) : base(chatClient, taskManager)
        {
            Attach(taskManager);
        }

        public void Attach(ITaskManager taskManager) {
            taskManager.OnAgentCardQuery = GetAgentCardAsync;
            taskManager.OnMessageReceived = ProcessMessageAsync;
        }
        /// <summary>
        /// Returns the agent card describing this agent's capabilities and skills for A2A discovery.
        /// </summary>
        public async override Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken) {
            return new AgentCard() {
                Name = "ShakespeareCrafter-Agent",
                Description = "Transforms user input into a William Shakespeare style text",
                Url = agentUrl,
                Version = "1.0.0",
                DefaultInputModes = ["application/json", "text/plain"],
                DefaultOutputModes = ["text/plain", "application/json"],
                Capabilities = new AgentCapabilities() { Streaming = true, PushNotifications = false },
                Skills =
                [
                    new AgentSkill()
                    {
                        Id = "craft-Shakespeare",
                        Name = "Shakespearean Text Generation",
                        Description = "Transforms user input into a William Shakespeare style text",
                        InputModes = ["text/plain"],
                        OutputModes = ["text/plain"],
                        Tags = ["Shakespeare", "creative", "text transformation"],
                        Examples = new List<string>()
                        {
                            @"{
                      ""description"": ""Transform into Shakespearean style"",
                      ""input"": { ""text"": ""To be or not to be"" },
                      ""output"": ""To beest, or not to beest: yond is the question.""
                    }"
                        },
                    }
                ],
            };
        }

        /// <summary>
        /// Processes an incoming A2A message and returns a response message in Shakespearean style.
        /// </summary>
        /// <param name="messageSendParams">Parameters containing the incoming message.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A Message object containing the Shakespearean style response.</returns>
        public override async Task<A2AResponse> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken ct) {
            // Get incoming message text
            string request = messageSendParams.Message.Parts.OfType<TextPart>().Select(p => p.Text)
                .FirstOrDefault() ?? "";

            // If no text provided, return an error message
            if(string.IsNullOrWhiteSpace(request)) {
                return new AgentMessage {
                    Role = MessageRole.Agent,
                    MessageId = Guid.NewGuid().ToString(),
                    ContextId = messageSendParams.Message.ContextId,
                    Parts = [new TextPart() { Text = "Please provide a phrase to transform into Shakespearean style." }]
                };
            }

            // Use IChatClient to generate Shakespearean style text
            var userMsg = new ChatMessage(ChatRole.User,
                $"Rewrite the following text in the style of William Shakespeare: '{request}'");

            var response = await InnerChatClient.GetResponseAsync([userMsg], new ChatOptions(), ct);

            string shakespeareText = response.Messages.Last().Text ?? string.Empty;

            // If the response is empty or null, return an error message
            if(string.IsNullOrEmpty(shakespeareText)) {
                return new AgentMessage {
                    Role = MessageRole.Agent,
                    MessageId = Guid.NewGuid().ToString(),
                    ContextId = messageSendParams.Message.ContextId,
                    Parts =
                    [
                        new TextPart()
                        {
                            Text = "Failed to generate Shakespearean text. Please try again with a different phrase."
                        }
                    ]
                };
            }

            // Create and return the Shakespearean style message
            return new AgentMessage() {
                Role = MessageRole.Agent,
                MessageId = Guid.NewGuid().ToString(),
                ContextId = messageSendParams.Message.ContextId,
                Parts = [new TextPart() { Text = shakespeareText }]
            };
        }
    }
}
