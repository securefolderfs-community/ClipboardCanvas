using System.Threading.Tasks;
using Windows.Storage;
using ClipboardCanvas.ReferenceItems;

namespace ClipboardCanvas.DataModels
{
    public class CanvasFile
    {
        #region Protected Members

        protected IStorageItem sourceItem;

        #endregion

        #region Public Properties

        public IStorageItem AssociatedItem { get; protected set; }

        public Task<IStorageItem> SourceItem => GetSourceItem();

        #endregion

        #region Constructor

        public CanvasFile(IStorageItem associatedItem)
            : this(associatedItem, null)
        {
        }

        public CanvasFile(IStorageItem associatedItem, IStorageItem sourceItem)
        {
            this.AssociatedItem = associatedItem;
            this.sourceItem = sourceItem;
        }

        #endregion

        #region Protected Helpers

        protected virtual async Task<IStorageItem> GetSourceItem()
        {
            if (sourceItem != null)
            {
                return sourceItem;
            }

            if (AssociatedItem is StorageFile file && ReferenceFile.IsReferenceFile(file))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);
                sourceItem = referenceFile.ReferencedItem;
            }
            else
            {
                sourceItem = AssociatedItem;
            }

            return sourceItem;
        }

        #endregion
    }
}
