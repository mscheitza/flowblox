using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.Web.OpenApi;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace FlowBlox.Core.Models.FlowBlocks.Web
{
    [Display(Name = "OpenApiRestFlowBlock_DisplayName", Description = "OpenApiRestFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class OpenApiRestFlowBlock : BaseResultFlowBlock
    {
        private string _openApiSource;
        private string _endpoint;
        private IReadOnlyList<OpenApiEndpointDescriptor> _endpoints = [];

        public OpenApiRestFlowBlock()
        {
            EndpointSuggestions = [];
            HeaderParameters = [];
            RequestParameters = [];
            ResultFields = [];
        }

        [Required]
        [Display(Name = "OpenApiRestFlowBlock_OpenApiSource", Description = "OpenApiRestFlowBlock_OpenApiSource_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFileSelection)]
        [FlowBloxTextBox(MultiLine = true, IsCodingMode = true, SyntaxHighlighting = "JSON")]
        public string OpenApiSource
        {
            get => _openApiSource;
            set
            {
                _openApiSource = value;
                ReloadDefinition(forceRefresh: false);
                OnPropertyChanged();
            }
        }

        [Required]
        [Display(Name = "OpenApiRestFlowBlock_Endpoint", Description = "OpenApiRestFlowBlock_Endpoint_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxTextBox(Suggestions = true, SuggestionMember = nameof(GetEndpointSuggestions))]
        public string Endpoint
        {
            get => _endpoint;
            set
            {
                _endpoint = value;
                ApplyEndpointDefaults();
                OnPropertyChanged();
            }
        }

        [Display(Name = "OpenApiRestFlowBlock_BaseUrl", Description = "OpenApiRestFlowBlock_BaseUrl_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string BaseUrl { get; set; }

        [Display(Name = "OpenApiRestFlowBlock_EndpointDescription", Description = "OpenApiRestFlowBlock_EndpointDescription_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBloxUI(ReadOnly = true)]
        [FlowBloxTextBox(MultiLine = true)]
        public string EndpointDescription { get; private set; }

        [Display(Name = "OpenApiRestFlowBlock_DefinitionError", Description = "OpenApiRestFlowBlock_DefinitionError_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [FlowBloxUI(ReadOnly = true)]
        [FlowBloxTextBox(MultiLine = true)]
        public string DefinitionError { get; private set; }

        [Display(Name = "OpenApiRestFlowBlock_BearerToken", Description = "OpenApiRestFlowBlock_BearerToken_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string BearerToken { get; set; }

        [Display(Name = "OpenApiRestFlowBlock_HeaderParameters", Description = "OpenApiRestFlowBlock_HeaderParameters_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        [FlowBloxUI(Factory = UIFactory.GridView, Operations = UIOperations.None)]
        [FlowBloxDataGrid(GridColumnMemberNames = [nameof(OpenApiParameterValue.Name), nameof(OpenApiParameterValue.Required), nameof(OpenApiParameterValue.Description), nameof(OpenApiParameterValue.Value)])]
        public ObservableCollection<OpenApiParameterValue> HeaderParameters { get; set; }

        [Display(Name = "OpenApiRestFlowBlock_RequestParameters", Description = "OpenApiRestFlowBlock_RequestParameters_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 7)]
        [FlowBloxUI(Factory = UIFactory.GridView, Operations = UIOperations.None)]
        [FlowBloxDataGrid(GridColumnMemberNames = [nameof(OpenApiParameterValue.Name), nameof(OpenApiParameterValue.Location), nameof(OpenApiParameterValue.Required), nameof(OpenApiParameterValue.Description), nameof(OpenApiParameterValue.Value)])]
        public ObservableCollection<OpenApiParameterValue> RequestParameters { get; set; }

        [Display(Name = "OpenApiRestFlowBlock_ContentType", Description = "OpenApiRestFlowBlock_ContentType_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 8)]
        public string ContentType { get; set; }

        [Display(Name = "OpenApiRestFlowBlock_Payload", Description = "OpenApiRestFlowBlock_Payload_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 9)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(MultiLine = true, IsCodingMode = true, SyntaxHighlighting = "JSON")]
        public string Payload { get; set; }

        [Display(Name = "OpenApiRestFlowBlock_ResultFields", Description = "OpenApiRestFlowBlock_ResultFields_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        [FlowBloxUI(Factory = UIFactory.GridView)]
        public ObservableCollection<ResultFieldByEnumValue<OpenApiRestDestinations>> ResultFields { get; set; }

        [JsonIgnore]
        public ObservableCollection<string> EndpointSuggestions { get; }

        public IEnumerable<string> GetEndpointSuggestions()
        {
            EnsureDefinitionLoaded();
            return EndpointSuggestions;
        }

        public void ReloadDefinition(bool forceRefresh)
        {
            EndpointSuggestions.Clear();
            _endpoints = [];
            DefinitionError = string.Empty;
            _endpoint = string.Empty;
            ClearEndpointDefaults();
            if (string.IsNullOrWhiteSpace(_openApiSource)) return;
            try
            {
                _endpoints = OpenApiDefinitionLoader.Load(_openApiSource, forceRefresh);
                foreach (var endpoint in _endpoints) EndpointSuggestions.Add(endpoint.DisplayName);
            }
            catch (Exception ex)
            {
                DefinitionError = ex.Message;
            }
            OnPropertyChanged(nameof(EndpointSuggestions));
            OnPropertyChanged(nameof(DefinitionError));
            OnPropertyChanged(nameof(Endpoint));
        }

        private void EnsureDefinitionLoaded()
        {
            if (_endpoints.Count == 0 && !string.IsNullOrWhiteSpace(_openApiSource))
                LoadDefinitionWithoutChangingSelection();
        }

        private void LoadDefinitionWithoutChangingSelection()
        {
            EndpointSuggestions.Clear();
            DefinitionError = string.Empty;
            try
            {
                _endpoints = OpenApiDefinitionLoader.Load(_openApiSource, forceRefresh: false);
                foreach (var endpoint in _endpoints) EndpointSuggestions.Add(endpoint.DisplayName);
            }
            catch (Exception ex)
            {
                _endpoints = [];
                DefinitionError = ex.Message;
            }
            OnPropertyChanged(nameof(EndpointSuggestions));
            OnPropertyChanged(nameof(DefinitionError));
        }

        private void ApplyEndpointDefaults()
        {
            ClearEndpointDefaults();
            EnsureDefinitionLoaded();
            var descriptor = FindEndpoint();
            if (descriptor == null) return;
            BaseUrl = descriptor.ServerUrl;
            EndpointDescription = descriptor.Summary;
            ContentType = descriptor.ContentType;
            Payload = descriptor.PayloadExample;
            foreach (var parameter in descriptor.Parameters)
            {
                var copy = new OpenApiParameterValue
                {
                    Name = parameter.Name, Location = parameter.Location, Required = parameter.Required,
                    Description = parameter.Description, Value = parameter.Value
                };
                if (string.Equals(copy.Location, "Header", StringComparison.OrdinalIgnoreCase)) HeaderParameters.Add(copy);
                else RequestParameters.Add(copy);
            }
            NotifyEndpointPropertiesChanged();
        }

        private void ClearEndpointDefaults()
        {
            BaseUrl = string.Empty;
            EndpointDescription = string.Empty;
            ContentType = string.Empty;
            Payload = string.Empty;
            HeaderParameters?.Clear();
            RequestParameters?.Clear();
            NotifyEndpointPropertiesChanged();
        }

        private void NotifyEndpointPropertiesChanged()
        {
            OnPropertyChanged(nameof(BaseUrl)); OnPropertyChanged(nameof(EndpointDescription));
            OnPropertyChanged(nameof(ContentType)); OnPropertyChanged(nameof(Payload));
            OnPropertyChanged(nameof(HeaderParameters)); OnPropertyChanged(nameof(RequestParameters));
        }

        private OpenApiEndpointDescriptor FindEndpoint()
            => _endpoints.FirstOrDefault(x => string.Equals(x.DisplayName, _endpoint, StringComparison.Ordinal));

        public override void OnAfterCreate()
        {
            CreateDestinationResultField(ResultFields, OpenApiRestDestinations.Payload);
            CreateDestinationResultField(ResultFields, OpenApiRestDestinations.StatusCode, FieldTypes.Integer);
            base.OnAfterCreate();
        }

        public override List<FieldElement> Fields => ResultFields.Where(x => x.EnumValue != null).Select(x => x.ResultField).ExceptNull().ToList();
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.web, 16, SKColors.SeaGreen);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.web, 32, SKColors.SeaGreen);
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Web;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Endpoint));
            properties.Add(nameof(BaseUrl));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data) => Invoke(runtime, data, () =>
        {
            runtime.Focus(this);
            Wait(runtime);
            SetParentElement(data);
            EnsureDefinitionLoaded();
            var descriptor = FindEndpoint() ?? throw new ValidationException("The selected OpenAPI endpoint is unavailable.");
            var result = InvokeEndpoint(descriptor).GetAwaiter().GetResult();
            var values = new ResultFieldByEnumValueResultBuilder<OpenApiRestDestinations>()
                .For(OpenApiRestDestinations.Payload, result.Payload)
                .For(OpenApiRestDestinations.StatusCode, result.StatusCode.ToString())
                .For(OpenApiRestDestinations.Status, result.Status)
                .For(OpenApiRestDestinations.ErrorMessage, result.ErrorMessage)
                .For(OpenApiRestDestinations.ResponseHeaders, result.ResponseHeaders)
                .For(OpenApiRestDestinations.Url, result.Url)
                .BuildSingleRow(ResultFields);
            GenerateResult(runtime, values);
        });

        private async Task<InvocationResult> InvokeEndpoint(OpenApiEndpointDescriptor descriptor)
        {
            var baseUrl = FlowBloxFieldHelper.ReplaceFieldsInString(BaseUrl ?? descriptor.ServerUrl)?.TrimEnd('/');
            var path = descriptor.Path;
            var query = new List<string>();
            var cookies = new List<string>();
            foreach (var parameter in RequestParameters)
            {
                var value = FlowBloxFieldHelper.ReplaceFieldsInString(parameter.Value ?? string.Empty);
                if (parameter.Required && string.IsNullOrWhiteSpace(value))
                    throw new ValidationException($"Required OpenAPI parameter '{parameter.Name}' is empty.");
                if (string.IsNullOrEmpty(value)) continue;
                if (string.Equals(parameter.Location, "Path", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace("{" + parameter.Name + "}", Uri.EscapeDataString(value), StringComparison.OrdinalIgnoreCase);
                else if (string.Equals(parameter.Location, "Cookie", StringComparison.OrdinalIgnoreCase))
                    cookies.Add($"{parameter.Name}={value}");
                else query.Add($"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(value)}");
            }
            var url = (baseUrl ?? string.Empty) + "/" + path.TrimStart('/');
            if (query.Count > 0) url += (url.Contains('?') ? "&" : "?") + string.Join("&", query);
            try
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod(descriptor.Method), url);
                var token = FlowBloxFieldHelper.ReplaceFieldsInString(BearerToken ?? string.Empty)?.Trim();
                if (token?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true) token = token[7..].Trim();
                if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                foreach (var parameter in HeaderParameters)
                {
                    var value = FlowBloxFieldHelper.ReplaceFieldsInString(parameter.Value ?? string.Empty);
                    if (parameter.Required && string.IsNullOrWhiteSpace(value))
                        throw new ValidationException($"Required OpenAPI header '{parameter.Name}' is empty.");
                    if (!string.IsNullOrEmpty(value)) request.Headers.TryAddWithoutValidation(parameter.Name, value);
                }
                if (cookies.Count > 0) request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", cookies));
                var payload = FlowBloxFieldHelper.ReplaceFieldsInString(Payload ?? string.Empty);
                if (!string.IsNullOrEmpty(payload)) request.Content = new StringContent(payload, Encoding.UTF8, string.IsNullOrWhiteSpace(ContentType) ? "application/json" : ContentType);
                using var response = await client.SendAsync(request).ConfigureAwait(false);
                var responsePayload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var headers = response.Headers.Concat(response.Content.Headers)
                    .ToDictionary(x => x.Key, x => string.Join(", ", x.Value), StringComparer.OrdinalIgnoreCase);
                return new InvocationResult(responsePayload, (int)response.StatusCode, response.ReasonPhrase ?? response.StatusCode.ToString(),
                    response.IsSuccessStatusCode ? string.Empty : responsePayload.Length > 0 ? responsePayload : response.ReasonPhrase ?? string.Empty,
                    JsonConvert.SerializeObject(headers, Formatting.Indented), response.RequestMessage?.RequestUri?.ToString() ?? url);
            }
            catch (HttpRequestException ex)
            {
                return new InvocationResult(string.Empty, 0, string.Empty, ex.Message, string.Empty, url);
            }
        }

        private sealed record InvocationResult(string Payload, int StatusCode, string Status, string ErrorMessage, string ResponseHeaders, string Url);
    }
}
