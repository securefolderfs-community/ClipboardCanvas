namespace ClipboardCanvas.DataModels
{
    public sealed class SearchDataModel
    {
        public readonly string phrase;

        public readonly int indexInSearch;

        public SearchDataModel(string phrase, int indexInSearch)
        {
            this.phrase = phrase;
            this.indexInSearch = indexInSearch;
        }
    }
}
