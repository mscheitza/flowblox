using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util
{
    public static class UriHelper
    {
        public static string ConcatUri(string uri1, string uri2)
        {
            if (string.IsNullOrEmpty(uri1))
                return uri2 ?? string.Empty;

            if (string.IsNullOrEmpty(uri2))
                return uri1 ?? string.Empty;

            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');

            return $"{uri1}/{uri2}";
        }
    }
}
