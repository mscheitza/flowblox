using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace FlowBlox.Core.Models.FlowBlocks.Authorization
{
    [Display(Name = "OAuthFlowBlock_DisplayName", Description = "OAuthFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class OAuthFlowBlock : BaseSingleResultFlowBlock
    {
        [Required]
        [Display(Name = "OAuthFlowBlock_TokenEndpoint", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string TokenEndpoint { get; set; }

        [Required]
        [Display(Name = "OAuthFlowBlock_ClientId", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "OAuthFlowBlock_ClientSecret", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox]
        public string ClientSecret { get; set; }

        [Display(Name = "OAuthFlowBlock_Scope", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Scope { get; set; }

        [Display(Name = "OAuthFlowBlock_Audience", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Audience { get; set; }

        [Display(Name = "OAuthFlowBlock_GrantType", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string GrantType { get; set; } = "client_credentials";

        [Display(Name = "OAuthFlowBlock_SendClientCredentialsInBody", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        public bool SendClientCredentialsInBody { get; set; } = true;

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.identifier, 16, SKColors.SteelBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.identifier, 32, SKColors.SteelBlue);

        public override FieldTypes DefaultResultFieldType => FieldTypes.Text;

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Authorization;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(TokenEndpoint));
            properties.Add(nameof(ClientId));
            properties.Add(nameof(Scope));
            properties.Add(nameof(Audience));
            properties.Add(nameof(GrantType));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var resolvedEndpoint = FlowBloxFieldHelper.ReplaceFieldsInString(TokenEndpoint ?? string.Empty)?.Trim();
                var resolvedClientId = FlowBloxFieldHelper.ReplaceFieldsInString(ClientId ?? string.Empty)?.Trim();
                var resolvedClientSecret = FlowBloxFieldHelper.ReplaceFieldsInString(ClientSecret ?? string.Empty);
                var resolvedScope = FlowBloxFieldHelper.ReplaceFieldsInString(Scope ?? string.Empty)?.Trim();
                var resolvedAudience = FlowBloxFieldHelper.ReplaceFieldsInString(Audience ?? string.Empty)?.Trim();
                var resolvedGrantType = FlowBloxFieldHelper.ReplaceFieldsInString(GrantType ?? string.Empty)?.Trim();

                if (string.IsNullOrWhiteSpace(resolvedEndpoint))
                    throw new ValidationException("OAuth token endpoint is empty.");

                if (string.IsNullOrWhiteSpace(resolvedClientId))
                    throw new ValidationException("OAuth client id is empty.");

                if (string.IsNullOrWhiteSpace(resolvedGrantType))
                    resolvedGrantType = "client_credentials";

                string token;
                try
                {
                    token = RequestToken(resolvedEndpoint, resolvedClientId, resolvedClientSecret, resolvedScope, resolvedAudience, resolvedGrantType).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    runtime.Report(ex.ToString());
                    CreateNotification(runtime, OAuthNotifications.AuthorizationFailed);
                    throw;
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    CreateNotification(runtime, OAuthNotifications.AuthorizationFailed);
                    throw new InvalidOperationException("OAuth authorization failed. Access token is empty.");
                }

                GenerateResult(runtime, [token]);
            });
        }

        private async Task<string> RequestToken(
            string tokenEndpoint,
            string clientId,
            string clientSecret,
            string scope,
            string audience,
            string grantType)
        {
            using var httpClient = new HttpClient();

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = grantType
            };

            if (!string.IsNullOrWhiteSpace(scope))
                form["scope"] = scope;

            if (!string.IsNullOrWhiteSpace(audience))
                form["audience"] = audience;

            if (SendClientCredentialsInBody)
            {
                form["client_id"] = clientId;
                form["client_secret"] = clientSecret ?? string.Empty;
            }
            else
            {
                var basicAuth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            }

            using var content = new FormUrlEncodedContent(form);
            using var response = await httpClient.PostAsync(tokenEndpoint, content).ConfigureAwait(false);
            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"OAuth token request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {payload}");

            if (string.IsNullOrWhiteSpace(payload))
                throw new InvalidOperationException("OAuth token request returned an empty response.");

            var json = JObject.Parse(payload);
            var accessToken = json["access_token"]?.Value<string>();
            return accessToken ?? string.Empty;
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(OAuthNotifications));
                return notificationTypes;
            }
        }

        public enum OAuthNotifications
        {
            [FlowBloxNotification(NotificationType = NotificationType.Error)]
            [Display(Name = "OAuthFlowBlock_Notification_AuthorizationFailed")]
            AuthorizationFailed
        }
    }
}
