using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Models.FlowBlocks.Web;

namespace FlowBlox.Core.Util.FlowBlocks
{
    internal class WebRequestPayloadCreator
    {
        private WebRequestFlowBlock webRequestFlowBlock;

        private const string Separator1 = "=";
        private const string Separator2 = "&";

        private string GetSeparator(string separatorWithSpecialChars)
        {
            return separatorWithSpecialChars
                .Replace("\\r", "\r")
                .Replace("\\n", "\n");
        }

        public WebRequestPayloadCreator(WebRequestFlowBlock extractorElement)
        {
            this.webRequestFlowBlock = extractorElement;
        }

        public string GetPayload(BaseRuntime runtime, Dictionary<string, string> fieldMap)
        {
            if (!string.IsNullOrEmpty(webRequestFlowBlock.Payload))
                return CreatePayload(fieldMap);
            else
                return CreatePayloadWithPostParameters(runtime, fieldMap);
        }

        private string CreatePayloadWithPostParameters(BaseRuntime runtime, Dictionary<string, string> postParameters)
        {
            string postContent = string.Empty;
            foreach (KeyValuePair<string, string> keyValue in FlowBloxFieldHelper.ReplaceFieldsInDictionary(postParameters))
            {
                runtime?.Report("HTTPPostParameter: Generated Key=" + keyValue.Key + " Value=" + keyValue.Value, Enums.FlowBloxLogLevel.Info);
                postContent += 
                    keyValue.Key + 
                    GetSeparator(Separator1) + 
                    keyValue.Value + 
                    GetSeparator(Separator2);
            }

            if (postContent.EndsWith(GetSeparator(Separator2)))
                postContent = postContent.Substring(0, postContent.Length - GetSeparator(Separator2).Length);

            return postContent;
        }

        private string CreatePayload(Dictionary<string, string> fieldMap)
        {
            string payload = webRequestFlowBlock.Payload;
            return FlowBloxFieldHelper.ReplaceFieldsInString(payload);
        }
    }
}
