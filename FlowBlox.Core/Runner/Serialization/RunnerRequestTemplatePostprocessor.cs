using FlowBlox.Core.Runner.Contracts;

namespace FlowBlox.Core.Runner.Serialization
{
    public static class RunnerRequestTemplatePostprocessor
    {
        public static string ResolveTemplates(RunnerRequest request, string responseFileTemplate)
        {
            var ctx = new RunnerPathTemplateContext
            {
                ProjectName = ResolveProjectName(request)
            };

            if (request?.OptionOverrides != null)
            {
                foreach (var key in request.OptionOverrides.Keys.ToList())
                {
                    request.OptionOverrides[key] = RunnerPathTemplateResolver.Resolve(request.OptionOverrides[key], ctx);
                }
            }

            return RunnerPathTemplateResolver.Resolve(responseFileTemplate, ctx);
        }

        private static string ResolveProjectName(RunnerRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request?.ProjectFile))
                return Path.GetFileNameWithoutExtension(request.ProjectFile);

            return !string.IsNullOrWhiteSpace(request?.ProjectSpaceGuid)
                ? request.ProjectSpaceGuid
                : "FlowBloxTask";
        }
    }
}