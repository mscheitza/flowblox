namespace FlowBlox.Core.Models.FlowBlocks.Web.InternalWebRequest
{
    public class ConfigurableWebRequestResult
    {
        public bool Success { get; set; }
        public string UrlCalled { get; set; }
        public ResponseBodyKind BodyKind { get; set; }
        public string Content { get; set; }
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
    }
}
