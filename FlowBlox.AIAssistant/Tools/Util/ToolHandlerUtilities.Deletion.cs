using FlowBlox.Core.Actions;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Deletion;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static partial class ToolHandlerUtilities
    {
        public static JArray BuildDeletionReport(DeletionReport report)
        {
            return new JArray(report.Dependencies.Select(dependency =>
            {
                var entry = new JObject
                {
                    ["referencingObject"] = GetDeletionObjectName(dependency.ReferencingObject),
                    ["referencingObjectType"] = dependency.ReferencingObject.GetType().Name,
                    ["property"] = dependency.Property.Name,
                    ["affectedObject"] = GetDeletionObjectName(dependency.ReferencedObject),
                    ["affectedObjectType"] = dependency.ReferencedObject.GetType().Name,
                    ["impact"] = dependency.Impact.ToString(),
                    ["reviewStatus"] = dependency.ReviewStatus.ToString(),
                    ["automaticallyRemoved"] = dependency.AutomaticallyRemoved
                };

                if (dependency.EditTarget != null &&
                    !ReferenceEquals(dependency.EditTarget, dependency.ReferencingObject))
                {
                    entry["referencingRoot"] = GetDeletionObjectName(dependency.EditTarget);
                    entry["referencingRootType"] = dependency.EditTarget.GetType().Name;
                }

                return entry;
            }));
        }

        public static void AddDeletionReportIfAny(JObject result, DeletionReport report)
        {
            if (report.HasDependencies)
                result["deletionReport"] = BuildDeletionReport(report);
        }

        public static void RegisterDeletionHistory(FlowBloxBaseAction action)
        {
            FlowBloxServiceLocator.Instance
                .GetService<IFlowBloxActionHistoryService>()
                ?.RegisterAction(action);
        }

        private static string GetDeletionObjectName(object value)
            => value is IFlowBloxComponent component && !string.IsNullOrWhiteSpace(component.Name)
                ? component.Name
                : value.ToString() ?? value.GetType().Name;

    }
}
