using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardCanvas.Extensions
{
    public static class CollectionExtensions
    {
        public static int IndexFitBounds(int itemsCount, int wantedIndex) => wantedIndex < 0 ? 0 : (wantedIndex >= itemsCount ? (itemsCount == 0 ? 0 : (itemsCount - 1)) : wantedIndex);

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || enumerable.Count() == 0;

        public static void DisposeClear<T>(this ICollection<T> collection)
        {
            for (int i = 0; i < collection.Count(); i++)
            {
                T item = collection.ElementAt(i);

                (item as IDisposable)?.Dispose();
                collection.Remove(item);
            }
        }
    }
}
