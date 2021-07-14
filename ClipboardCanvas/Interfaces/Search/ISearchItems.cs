using System.Collections.Generic;

namespace ClipboardCanvas.Interfaces.Search
{
    public interface ISearchItems
    {
        int Count { get; }

        int CurrentIndex { get; }

        IEnumerable<ISearchItem> CompareItemsToPhrase(string phrase);

        int IndexOfItemInCollection(ISearchItem item);

        void SetSelectedIndex(int index);
    }
}
