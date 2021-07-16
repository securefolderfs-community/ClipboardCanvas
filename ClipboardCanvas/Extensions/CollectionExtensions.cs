using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardCanvas.Extensions
{
    public static class CollectionExtensions
    {
        public static int IndexFitBounds(int itemsCount, int wantedIndex) => wantedIndex < 0 ? 0 : (wantedIndex >= itemsCount ? (itemsCount == 0 ? 0 : (itemsCount - 1)) : wantedIndex);

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || enumerable.Count() == 0;

        public static void AddFront<T>(this IList<T> list, T item) => list.Insert(0, item);

        public static void DisposeClear<T>(this ICollection<T> collection)
        {
            if (collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                (item as IDisposable)?.Dispose();
            }

            collection.Clear();
        }
    }
}
