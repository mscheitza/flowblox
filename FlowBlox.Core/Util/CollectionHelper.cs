using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util
{
    public static class CollectionHelper
    {
        public static void AddIfNotExists<T>(this ICollection<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        public static void AddIfNotNull<T>(this ICollection<T> list, T item)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }
    }
}
