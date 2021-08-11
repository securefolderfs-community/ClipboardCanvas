using Windows.Storage;
using System.Threading.Tasks;

using ClipboardCanvas.Models;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Contexts;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionItemViewModel : CanvasItem, ICollectionItemModel
    {
        public BaseContentTypeModel ContentType { get; set; }

        public OperationContext OperationContext { get; set; }

        public CollectionItemViewModel(IStorageItem item)
            : this(item, null)
        {
        }

        public CollectionItemViewModel(IStorageItem item, BaseContentTypeModel contentType)
            : base(item)
        {
            this.ContentType = contentType;
            this.OperationContext = new OperationContext();
        }

        public async Task OpenFile()
        {
            await StorageHelpers.OpenFile(await SourceItem);
        }

        public async Task OpenContainingFolder()
        {
            await OpenContainingFolder(true);
        }

        public async Task OpenContainingFolder(bool checkForReference)
        {
            if (checkForReference)
            {
                await StorageHelpers.OpenContainingFolder(await SourceItem);
            }
            else
            {
                await StorageHelpers.OpenContainingFolder(AssociatedItem);
            }
        }
    }
}
