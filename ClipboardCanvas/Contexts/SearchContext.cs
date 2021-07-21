namespace ClipboardCanvas.Contexts
{
    public sealed class SearchContext
    {
        public readonly string phrase;

        public readonly int indexInSearch;

        public SearchContext(string phrase, int indexInSearch)
        {
            this.phrase = phrase;
            this.indexInSearch = indexInSearch;
        }
    }
}
