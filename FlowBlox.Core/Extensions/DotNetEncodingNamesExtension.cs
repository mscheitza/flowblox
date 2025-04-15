using FlowBlox.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
