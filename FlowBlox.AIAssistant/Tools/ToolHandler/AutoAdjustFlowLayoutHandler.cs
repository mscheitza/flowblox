using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.FlowBlocks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class AutoAdjustFlowLayoutHandler : ToolHandlerBase
    {
        public override string Name => "AutoAdjustFlowLayout";
        public override bool IsLayoutRelevantForAutoAdjustment => true;

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Automatically re-centers and aligns flow blocks based on their graph connections.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var payload = TryInvokeWithLoader(ExecuteAutoAdjustCore);
            return Task.FromResult(ToolHandlerUtilities.Ok(payload));
        }

        private static JObject ExecuteAutoAdjustCore()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            if (registry?.GetStartFlowBlock() == null)
            {
                return new JObject
                {
                    ["updated"] = 0,
                    ["total"] = registry?.GetFlowBlocks()?.Count() ?? 0,
                    ["components"] = 0,
                    ["message"] = "No start flow block was found. Automatic alignment was not performed."
                };
            }

            var result = FlowBlockAutoLayoutAdjuster.AdjustCurrentRegistryLayout();
            var moveActions = FlowBlockAutoLayoutAdjuster.GetRecordedMoveActions();
            FlowBloxServiceLocator.Instance
                .GetService<IFlowBloxActionHistoryService>()
                ?.RegisterAutoLayoutMoves(moveActions);

            return new JObject
            {
                ["updated"] = result.UpdatedFlowBlocks,
                ["total"] = result.TotalFlowBlocks,
                ["components"] = result.ComponentsProcessed
            };
        }

        private static JObject TryInvokeWithLoader(Func<JObject> action)
        {
            try
            {
                var appWindowType = Type.GetType("FlowBlox.AppWindow.AppWindow, FlowBlox", throwOnError: false);
                if (appWindowType == null)
                    return action();

                var instanceProperty = appWindowType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var appWindowInstance = instanceProperty?.GetValue(null);
                if (appWindowInstance == null)
                    return action();

                var invokeWithLoaderMethod = appWindowType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m =>
                        m.Name == "InvokeWithLoader" &&
                        m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(Func<>).MakeGenericType(m.GetGenericArguments()[0]));

                if (invokeWithLoaderMethod == null)
                    return action();

                var closedMethod = invokeWithLoaderMethod.MakeGenericMethod(typeof(JObject));
                var result = closedMethod.Invoke(appWindowInstance, [action]);
                return result as JObject ?? action();
            }
            catch
            {
                return action();
            }
        }
    }
}