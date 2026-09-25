using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowBlox.Core.Models.Components.Modifier
{
    [Display(Name = "HashModifier_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class HashModifier : ModifierBase
    {
        [Display(Name = "HashModifier_Algorithm", Description = "HashModifier_Algorithm_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.ComboBox)]
        public CryptographicHashAlgorithm Algorithm { get; set; } = CryptographicHashAlgorithm.SHA256;

        [Display(Name = "HashModifier_OutputEncoding", Description = "HashModifier_OutputEncoding_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.ComboBox)]
        public CryptographicOutputEncoding OutputEncoding { get; set; } = CryptographicOutputEncoding.HexLowerCase;

        public override string Modify(BaseRuntime runtime, string value)
        {
            using var algorithm = CryptographicModifierSupport.CreateHash(Algorithm);
            return CryptographicModifierSupport.Encode(
                algorithm.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)),
                OutputEncoding);
        }

        public override string ToString() => string.Format(
            FlowBloxResourceUtil.GetLocalizedString(nameof(HashModifier), nameof(ObjectDisplayName)),
            Algorithm.GetDisplayName(), OutputEncoding.GetDisplayName());
    }
}
