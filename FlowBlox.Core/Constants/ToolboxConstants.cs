namespace FlowBlox.Core.Constants
{
    public static class ToolboxConstants
    {
        public const string AIPromptTemplatesCategory = "AIPromptTemplates";

        public const string SystemJsonOutputPromptTemplateName = "System - JSON Output";
        public const string SystemJsonOutputPromptTemplateContent =
            "Return exactly one valid JSON object.\r\n" +
            "Do not include explanations, Markdown, code fences, alternatives, examples, or text before/after the JSON.\r\n" +
            "If you are unsure, still return the single best JSON object.";

        public const string SystemFlowBloxQuickUpdatePromptTemplateName = "System - FlowBlox Quick Update JSON Output";
        public const string SystemFlowBloxQuickUpdatePromptTemplateContent =
            "Return exactly one FlowBlox Quick Update JSON object.\r\n" +
            "Return only the JSON object, no Markdown, no comments, no alternative JSON objects.\r\n" +
            "If multiple configurations are possible, choose the single most likely configuration.";

        public const string SystemXmlOutputPromptTemplateName = "System - XML Output";
        public const string SystemXmlOutputPromptTemplateContent =
            "Return exactly one valid XML document.\r\n" +
            "Use a single root element.\r\n" +
            "Do not include explanations, Markdown, code fences, alternatives, examples, or text before/after the XML.\r\n" +
            "If you are unsure, still return the single best XML document.";

        public const string FlowBloxQuickUpdateConfiguratorName = "FlowBlox Quick Update Configurator";
    }
}
