using System.Collections;
using System.Linq;

namespace ClipboardCanvas.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty(this ICollection collection) => collection.Count == 0;

        public static int IndexFitBounds(int itemsCount, int wantedIndex) => wantedIndex < 0 ? 0 : (wantedIndex > itemsCount ? ( itemsCount == 0 ? 0 : itemsCount - 1) : wantedIndex);
    }
}
