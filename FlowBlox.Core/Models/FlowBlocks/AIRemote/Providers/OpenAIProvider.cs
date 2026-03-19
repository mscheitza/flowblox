using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    [Display(Name = "TypeNames_OpenAIProvider", ResourceType = typeof(FlowBloxTexts))]
    public sealed class OpenAIProvider : AIProviderBase
    {
        [Display(Name = "OpenAIProvider_OrganizationId", Description = "OpenAIProvider_OrganizationId_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public string OrganizationId { get; set; }

        [Display(Name = "OpenAIProvider_ProjectId", Description = "OpenAIProvider_ProjectId_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 11)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public string ProjectId { get; set; }

        [Display(Name = "OpenAIProvider_StoreResponses", Description = "OpenAIProvider_StoreResponses_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 12)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool StoreResponses { get; set; } = false;

        public override string ProviderType => "OpenAI";
        public override bool SupportsNativeResponseContinuation => true;

        private HttpClient _http;

        public OpenAIProvider()
        {
            BaseUrl = "https://api.openai.com/v1";
            DefaultModel = "gpt-5.2";
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
                    Error = "OpenAI API key is empty after field resolution."
                };
            }

            var resolvedBaseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl);
            var resolvedOrganizationId = FlowBloxFieldHelper.ReplaceFieldsInString(OrganizationId);
            var resolvedProjectId = FlowBloxFieldHelper.ReplaceFieldsInString(ProjectId);
            var resolvedModel = FlowBloxFieldHelper.ReplaceFieldsInString(request.Model);

            var urlBase = string.IsNullOrWhiteSpace(resolvedBaseUrl) ? 
                "https://api.openai.com/v1" : 
                resolvedBaseUrl.TrimEnd('/');

            var url = $"{urlBase}/responses";

            using var msg = new HttpRequestMessage(HttpMethod.Post, url);

            // Auth: Authorization: Bearer
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resolvedApiKey);

            // Optional org/project headers
            if (!string.IsNullOrWhiteSpace(resolvedOrganizationId))
                msg.Headers.TryAddWithoutValidation("OpenAI-Organization", resolvedOrganizationId);

            if (!string.IsNullOrWhiteSpace(resolvedProjectId))
                msg.Headers.TryAddWithoutValidation("OpenAI-Project", resolvedProjectId);

            // Build request body (Responses API)
            var body = new JObject
            {
                ["model"] = resolvedModel,
                ["input"] = request.Prompt,
                ["store"] = ShouldStoreResponses(request)
            };

            if (!string.IsNullOrWhiteSpace(request.SystemInstruction))
                body["instructions"] = request.SystemInstruction;

            if (!string.IsNullOrWhiteSpace(request.PreviousResponseId))
                body["previous_response_id"] = request.PreviousResponseId;

            if (request.Temperature is >= 0 and <= 2)
                body["temperature"] = request.Temperature;

            if (request.MaxTokens.HasValue && request.MaxTokens.Value > 0)
                body["max_output_tokens"] = request.MaxTokens.Value;

            if (request.Meta != null && request.Meta.Count > 0)
            {
                var meta = new JObject();
                foreach (var kv in request.Meta)
                {
                    if (kv.Value == null) continue;
                    meta[kv.Key] = kv.Value.ToString();
                }
                if (meta.HasValues)
                    body["metadata"] = meta;
            }

            msg.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                // OpenAI returns error JSON; pass through for debugging
                return new AIResponse
                {
                    Success = false,
                    Error = $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}: {json}"
                };
            }

            var text = TryExtractOutputText(json);
            var responseId = TryExtractResponseId(json);

            return new AIResponse
            {
                Success = true,
                Text = text ?? string.Empty,
                ResponseId = responseId
            };
        }

        private bool ShouldStoreResponses(AIRequest request)
        {
            if (StoreResponses)
                return true;

            if (!string.IsNullOrWhiteSpace(request?.PreviousResponseId))
                return true;

            if (request?.Meta == null)
                return false;

            if (!request.Meta.TryGetValue("RequireResponseStorage", out var value))
                return false;

            return value switch
            {
                bool b => b,
                string s when bool.TryParse(s, out var parsed) => parsed,
                _ => false
            };
        }

        private static string TryExtractResponseId(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
                return null;

            var root = JObject.Parse(responseJson);
            return root["id"]?.Value<string>();
        }

        private static string TryExtractOutputText(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
                return null;

            var root = JObject.Parse(responseJson);

            // OpenAI Responses API output structure:
            //
            // {
            //   "output": [
            //     {
            //       "type": "message",
            //       "role": "assistant",
            //       "content": [
            //         {
            //           "type": "output_text",
            //           "text": "The generated text from the model."
            //         }
            //       ]
            //     }
            //   ]
            // }

            var outputs = root["output"] as JArray;
            if (outputs == null) return null;

            var sb = new StringBuilder();

            foreach (var outputItem in outputs)
            {
                var content = outputItem?["content"] as JArray;
                if (content == null) continue;

                foreach (var c in content)
                {
                    var type = c?["type"]?.Value<string>();
                    if (type == "output_text")
                    {
                        var t = c?["text"]?.Value<string>();
                        if (!string.IsNullOrEmpty(t))
                        {
                            if (sb.Length > 0) sb.AppendLine();
                            sb.Append(t);
                        }
                    }
                }
            }

            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}
