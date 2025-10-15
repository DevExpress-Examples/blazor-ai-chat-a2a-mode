using A2A;
using A2AServer.Agents.Base;
using Microsoft.Extensions.AI;

namespace PoemAgentServer.Agents {
    /// <summary>
    /// An agent that generates poems based on provided keywords using AI chat capabilities.
    /// </summary>
    public class PoemAgent : BaseMessageAgent {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoemAgent"/> class.
        /// </summary>
        /// <param name="taskManager">The task manager to attach to this agent.</param>
        public PoemAgent(IChatClient chatClient, ITaskManager taskManager) : base(chatClient, taskManager) {
            Attach(taskManager);
        }

        public void Attach(ITaskManager taskManager) {
            taskManager.OnAgentCardQuery = GetAgentCardAsync;
            taskManager.OnMessageReceived = ProcessMessageAsync;
        }
        public async override Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken) {
            return new AgentCard() {
                Name = "PoemCrafter‑Agent",
                Description =
                    "Generates a short poem (5–7 lines) based on provided keywords, weaving them into lyrical style with optional rhyme or rhythm.",
                Url = agentUrl,
                Version = "1.0.0",
                DefaultInputModes = ["application/json", "text/plain"],
                DefaultOutputModes = ["text/plain", "application/json"],
                Capabilities = new AgentCapabilities() { Streaming = true, PushNotifications = false },
                Skills =
                [
                    new AgentSkill()
                    {
                        Id = "craft-poem",
                        Name = "Keyword-based poem generation",
                        Description =
                            "Generate a short poetic verse (3–5 lines) incorporating the given keywords with optional rhyme or rhythm.",
                        InputModes = ["text/plain"],
                        OutputModes = ["text/plain"],
                        Tags = ["poem", "creative", "keywords"],
                        Examples = new List<string>()
                        {
                            @"{
                      ""description"": ""A poem using keywords 'moon', 'whisper', 'oceans'"",
                      ""input"": { ""keywords"": [""moon"", ""whisper"", ""oceans""] },
                      ""output"": ""Moonlight whispers upon the ocean’s crest\nA silent dream where silver tides rest\n…""
                    }"
                        },
                    }
                ],
            };
        }
        /// <summary>
        /// Processes an incoming message and returns a poem message using the inner chat client.
        /// </summary>
        /// <param name="messageSendParams">Parameters containing the incoming message.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A2A Message containing the poem or an error message.</returns>
        public override async Task<A2AResponse> ProcessMessageAsync(MessageSendParams messageSendParams,
            CancellationToken ct) {
            // Get incoming message text
            string request = messageSendParams.Message.Parts.OfType<TextPart>().Select(p => p.Text)
                .FirstOrDefault() ?? "";

            // Parse keywords from the request
            List<string> keywords = request
                .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Take(5)
                .ToList();

            // If no keywords provided, return an error message
            if(keywords.Count < 2) {
                return new AgentMessage {
                    Role = MessageRole.Agent,
                    MessageId = Guid.NewGuid().ToString(),
                    ContextId = messageSendParams.Message.ContextId,
                    Parts =
                    [
                        new TextPart()
                        {
                            Text =
                                "Please provide at least one keyword (e.g. \"stars, silence, dream\") for poem generation."
                        }
                    ]
                };
            }

            // Use IChatClient to generate a mini-poem
            var userMsg = new ChatMessage(ChatRole.User,
                $"Please generate a short poem (5–7 lines) that weaves the following keywords into a lyrical style, with optional rhyme or rhythm: {string.Join(", ", keywords)}");

            var response = await InnerChatClient.GetResponseAsync([userMsg], new ChatOptions(), ct);

            string poem = response.Messages.Last().Text ?? string.Empty;

            // If the response is empty or null, return an error message
            if(string.IsNullOrEmpty(response.Messages.Last().Text)) {
                return new AgentMessage {
                    Role = MessageRole.Agent,
                    MessageId = Guid.NewGuid().ToString(),
                    ContextId = messageSendParams.Message.ContextId,
                    Parts =
                    [
                        new TextPart() { Text = "Failed to generate a poem. Please try again with different keywords." }
                    ]
                };
            }

            // Create and return an echo message
            return new AgentMessage() {
                Role = MessageRole.Agent,
                MessageId = Guid.NewGuid().ToString(),
                ContextId = messageSendParams.Message.ContextId,
                Parts = [new TextPart() { Text = poem }]
            };
        }
    }
}
