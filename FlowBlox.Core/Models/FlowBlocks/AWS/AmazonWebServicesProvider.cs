using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Constants;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.Core.Models.FlowBlocks.AWS
{
    [Display(Name = "AmazonWebServicesProvider_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("AmazonWebServicesProvider_DisplayName_Plural", typeof(FlowBloxTexts))]
    public sealed class AmazonWebServicesProvider : ManagedObject
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.cube_outline, 16, new SKColor(35, 92, 148));

        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.cube_outline, 32, new SKColor(35, 92, 148));

        [Required]
        [Display(Name = "AmazonWebServicesProvider_ApiKey", Description = "AmazonWebServicesProvider_ApiKey_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox]
        public string ApiKey { get; set; }

        [Required]
        [Display(Name = "AmazonWebServicesProvider_SecretKey", Description = "AmazonWebServicesProvider_SecretKey_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox]
        public string SecretKey { get; set; }

        [Required]
        [Display(Name = "AmazonWebServicesProvider_Region", Description = "AmazonWebServicesProvider_Region_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string Region { get; set; } = "eu-central-1";

        [Display(Name = "AmazonWebServicesProvider_S3ServiceUrl", Description = "AmazonWebServicesProvider_S3ServiceUrl_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBloxUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string S3ServiceUrl { get; set; }

        [Display(Name = "AmazonWebServicesProvider_S3ForcePathStyle", Description = "AmazonWebServicesProvider_S3ForcePathStyle_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public bool S3ForcePathStyle { get; set; }

        public TClient CreateClient<TClient>()
        {
            var constructor = typeof(TClient).GetConstructor([typeof(AWSCredentials), typeof(RegionEndpoint)]);
            if (constructor == null)
                throw new InvalidOperationException($"AWS client '{typeof(TClient).FullName}' does not expose a credentials/region constructor.");

            try
            {
                return (TClient)constructor.Invoke([CreateCredentials(), ResolveRegionEndpoint()]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        public AWSCredentials CreateCredentials()
        {
            var resolvedApiKey = FlowBloxFieldHelper.ReplaceFieldsInString(ApiKey ?? string.Empty);
            var resolvedSecretKey = FlowBloxFieldHelper.ReplaceFieldsInString(SecretKey ?? string.Empty);

            if (string.IsNullOrWhiteSpace(resolvedApiKey))
                throw new InvalidOperationException("AWS API key is empty after field resolution.");

            if (string.IsNullOrWhiteSpace(resolvedSecretKey))
                throw new InvalidOperationException("AWS secret key is empty after field resolution.");

            return new BasicAWSCredentials(resolvedApiKey, resolvedSecretKey);
        }

        public AmazonS3Client CreateS3Client()
            => new AmazonS3Client(CreateCredentials(), CreateS3Configuration());

        public AmazonS3Config CreateS3Configuration()
        {
            var resolvedRegion = ResolveRegionEndpoint();
            var resolvedServiceUrl = FlowBloxFieldHelper.ReplaceFieldsInString(S3ServiceUrl ?? string.Empty)?.Trim();
            var configuration = new AmazonS3Config
            {
                ForcePathStyle = S3ForcePathStyle
            };

            if (string.IsNullOrWhiteSpace(resolvedServiceUrl))
            {
                configuration.RegionEndpoint = resolvedRegion;
            }
            else
            {
                configuration.ServiceURL = resolvedServiceUrl;
                configuration.AuthenticationRegion = resolvedRegion.SystemName;
            }

            return configuration;
        }

        public RegionEndpoint ResolveRegionEndpoint()
        {
            var resolvedRegion = FlowBloxFieldHelper.ReplaceFieldsInString(Region ?? string.Empty)?.Trim();
            if (string.IsNullOrWhiteSpace(resolvedRegion))
                throw new InvalidOperationException("AWS region is empty after field resolution.");

            return RegionEndpoint.GetBySystemName(resolvedRegion);
        }
    }
}
