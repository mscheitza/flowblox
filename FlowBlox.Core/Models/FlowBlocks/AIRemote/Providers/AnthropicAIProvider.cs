using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "AnthropicAIProvider_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("AnthropicAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class AnthropicAIProvider : AIProviderBase
    {
        public override string ProviderType => "Anthropic";

        // Anthropic supports a container concept for conversation state.
        public override bool SupportsNativeResponseContinuation => true;

        private HttpClient _http;

        public AnthropicAIProvider()
        {
            BaseUrl = "https://api.anthropic.com/v1";
            DefaultModel = "claude-opus-4-5";
            TimeoutSeconds = 60;
        }

        protected override void OnBeforeExecution()
        {
            _http?.Dispose();

            _http = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected override void OnAfterExecution()
        {
            _http?.Dispose();
            _http = null;
        }

        protected override async Task<AIResponse> ExecuteCoreAsync(AIRequest request, CancellationToken ct)
        {
            var resolvedApiKey = FlowBloxFieldHelper.ReplaceFieldsInString(ApiKey);
            if (string.IsNullOrWhiteSpace(resolvedApiKey))
            {
                return new AIResponse
                {
                    Success = false,
                    Error = "Anthropic API key is empty after field resolution."
                };
            }

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);

            var urlBase = string.IsNullOrWhiteSpace(resolvedBaseUrl)
                ? "https://api.anthropic.com/v1"
                : resolvedBaseUrl.TrimEnd('/');

            var url = $"{urlBase}/messages";

            using var msg = new HttpRequestMessage(HttpMethod.Post, url);
            msg.Headers.TryAddWithoutValidation("x-api-key", resolvedApiKey);
            msg.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");

            var body = new JObject
            {
                ["model"] = resolvedModel,
                ["max_tokens"] = request.MaxTokens.GetValueOrDefault(1024),
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = request.Prompt ?? string.Empty
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(request.SystemInstruction))
                body["system"] = request.SystemInstruction;

            if (request.Temperature is >= 0 and <= 1)
                body["temperature"] = request.Temperature;

            if (!string.IsNullOrWhiteSpace(request.PreviousResponseId))
                body["container"] = request.PreviousResponseId;

            if (request.Meta != null && request.Meta.Count > 0)
            {
                var metadata = new JObject();
                foreach (var kv in request.Meta)
                {
                    if (kv.Value == null)
                        continue;

                    metadata[kv.Key] = kv.Value.ToString();
                }

                if (metadata.HasValues)
                    body["metadata"] = metadata;
            }

            msg.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(msg, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new AIResponse
                {
                    Success = false,
                    Error = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {json}"
                };
            }

            var parsed = JObject.Parse(json);
            var text = TryExtractOutputText(parsed);

            return new AIResponse
            {
                Success = true,
                Text = text ?? string.Empty,
                ResponseId = parsed["container"]?.Value<string>() ?? parsed["id"]?.Value<string>(),
                PromptTokens = parsed["usage"]?["input_tokens"]?.Value<int?>(),
                CompletionTokens = parsed["usage"]?["output_tokens"]?.Value<int?>()
            };
        }

        private static string TryExtractOutputText(JObject root)
        {
            var content = root?["content"] as JArray;
            if (content == null || content.Count == 0)
                return null;

            var sb = new StringBuilder();
            foreach (var block in content.OfType<JObject>())
            {
                var blockType = block["type"]?.Value<string>();
                if (!string.Equals(blockType, "text", StringComparison.OrdinalIgnoreCase))
                    continue;

                var text = block["text"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(text);
            }

            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}
