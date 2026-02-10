using FlowBlox.Core.Enums;
using System.Text;

namespace FlowBlox.Core.Extensions
{
    public static class DotNetEncodingNamesExtension
    {
        public static Encoding ToEncoding(this DotNetEncodingNames encodingName)
        {
            return encodingName == DotNetEncodingNames.Default
                ? Encoding.Default
                : Encoding.GetEncoding(encodingName.ToString());
        }
    }
}
