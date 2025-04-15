using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlowBlox.Core.Extensions
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Adds a range of items to the ObservableCollection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The target ObservableCollection.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
