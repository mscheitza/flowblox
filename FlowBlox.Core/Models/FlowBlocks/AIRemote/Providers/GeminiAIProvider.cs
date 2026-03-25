using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "GeminiAIProvider_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("GeminiAIProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class GeminiAIProvider : AIProviderBase
    {
        public override string ProviderType => "Gemini";

        private HttpClient _http;

        public GeminiAIProvider()
        {
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
            DefaultModel = "gemini-3-flash";
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
                    Error = "Gemini API key is empty after field resolution."
                };
            }

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);
            if (string.IsNullOrWhiteSpace(resolvedModel))
                resolvedModel = "gemini-2.5-flash";

            var urlBase = string.IsNullOrWhiteSpace(resolvedBaseUrl)
                ? "https://generativelanguage.googleapis.com/v1beta"
                : resolvedBaseUrl.TrimEnd('/');

            var url = $"{urlBase}/models/{Uri.EscapeDataString(resolvedModel)}:generateContent?key={Uri.EscapeDataString(resolvedApiKey)}";

            using var msg = new HttpRequestMessage(HttpMethod.Post, url);

            var body = new JObject
            {
                ["contents"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["parts"] = new JArray
                        {
                            new JObject { ["text"] = request.Prompt ?? string.Empty }
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(request.SystemInstruction))
            {
                body["systemInstruction"] = new JObject
                {
                    ["parts"] = new JArray
                    {
                        new JObject { ["text"] = request.SystemInstruction }
                    }
                };
            }

            var generationConfig = new JObject();
            if (request.Temperature >= 0)
                generationConfig["temperature"] = request.Temperature;

            if (request.MaxTokens.HasValue && request.MaxTokens.Value > 0)
                generationConfig["maxOutputTokens"] = request.MaxTokens.Value;

            if (generationConfig.HasValues)
                body["generationConfig"] = generationConfig;

            msg.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                return new AIResponse
                {
                    Success = false,
                    Error = $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}: {json}"
                };
            }

            var parsed = JObject.Parse(json);
            var text = TryExtractOutputText(parsed);
            if (string.IsNullOrWhiteSpace(text))
            {
                var blocked = parsed["promptFeedback"]?["blockReason"]?.Value<string>();
                var message = parsed["promptFeedback"]?["blockReasonMessage"]?.Value<string>();
                if (!string.IsNullOrWhiteSpace(blocked))
                {
                    return new AIResponse
                    {
                        Success = false,
                        Error = string.IsNullOrWhiteSpace(message)
                            ? $"Gemini prompt blocked: {blocked}"
                            : $"Gemini prompt blocked: {blocked} ({message})"
                    };
                }
            }

            return new AIResponse
            {
                Success = true,
                Text = text ?? string.Empty,
                ResponseId = parsed["responseId"]?.Value<string>(),
                PromptTokens = parsed["usageMetadata"]?["promptTokenCount"]?.Value<int?>(),
                CompletionTokens = parsed["usageMetadata"]?["candidatesTokenCount"]?.Value<int?>()
            };
        }

        private static string TryExtractOutputText(JObject root)
        {
            var candidates = root?["candidates"] as JArray;
            if (candidates == null || candidates.Count == 0)
                return null;

            var sb = new StringBuilder();

            foreach (var candidate in candidates.OfType<JObject>())
            {
                var parts = candidate["content"]?["parts"] as JArray;
                if (parts == null)
                    continue;

                foreach (var part in parts.OfType<JObject>())
                {
                    var text = part["text"]?.Value<string>();
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.Append(text);
                }
            }

            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}
