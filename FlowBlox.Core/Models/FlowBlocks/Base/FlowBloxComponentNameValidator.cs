using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public static class FlowBloxComponentNameValidator
    {
        public static ValidationResult Validate(string name, ValidationContext context)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new ValidationResult(
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("FlowBloxComponent_Validation_NameRequired"),
                        name
                    ), [context.MemberName]);
            }

            if (context.ObjectInstance is FieldElement fieldElement && !fieldElement.UserField)
            {
                return ValidateRuntimeField(fieldElement, name, context);
            }

            return ValidateName(name, context);
        }

        private static ValidationResult ValidateName(string name, ValidationContext context)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var instance = context.ObjectInstance;

            IEnumerable<IFlowBloxComponent> existingComponents;

            switch (instance)
            {
                case BaseFlowBlock:
                    existingComponents = registry.GetFlowBlocks();
                    break;

                case IManagedObject:
                    existingComponents = registry.GetManagedObjects();
                    break;

                default:
                    return new ValidationResult(
                        $"Cannot validate name uniqueness for type '{instance.GetType().Name}'", [context.MemberName]);
            }

            var isDuplicate = existingComponents
                .Where(x => !ReferenceEquals(x, instance))
                .Any(x => x.Name == name);

            if (isDuplicate)
            {
                return new ValidationResult(
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("FlowBloxComponentNameValidator_NameAlreadyExists"),
                        name
                    ), [context.MemberName]);
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateRuntimeField(FieldElement fieldElement, string name, ValidationContext context)
        {
            var source = fieldElement.Source;
            if (source == null)
                return new ValidationResult(FlowBloxResourceUtil.GetLocalizedString("FlowBloxComponentNameValidator_FieldSourceNotSet"), [context.MemberName]);

            bool nameExistsInOtherFieldsOfSource = source.Fields
                .Where(x => x.Name != fieldElement.Name)
                .Any(x => x.Name == name);

            if (nameExistsInOtherFieldsOfSource)
            {
                return new ValidationResult(
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("FlowBloxComponentNameValidator_NameAlreadyExists"),
                        name
                    ), [context.MemberName]);
            }

            return ValidationResult.Success;
        }
    }
}
