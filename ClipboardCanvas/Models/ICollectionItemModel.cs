using Windows.Storage;
using System.Threading.Tasks;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.Models
{
    /// <summary>
    /// This interface is an item model that's within <see cref="ICollectionModel"/>
    /// </summary>
    public interface ICollectionItemModel
    {
        IStorageItem Item { get; }

        Task<IStorageItem> SourceItem { get; }

        BaseContentTypeModel ContentType { get; set; }

        Task OpenFile();

        Task OpenContainingFolder();

        Task OpenContainingFolder(bool checkForReference);
    }
}
