using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.WebRequest
{
    public class WebRequestInvocationResult
    {
        public bool Success { get; set; }
        public string UrlCalled { get; set; }
        public string Content { get; set; }
        public ResponseBodyKind BodyKind { get; set; }
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }

        public WebRequestInvocationResult(ConfigurableWebRequestResult src) : this(src.Success)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            UrlCalled = src.UrlCalled;
            Content = src.Content;
            BodyKind = src.BodyKind;
            FileName = src.FileName;
            Bytes = src.Bytes;
        }

        public WebRequestInvocationResult(bool success)
        {
            Success = success;
        }
    }
}