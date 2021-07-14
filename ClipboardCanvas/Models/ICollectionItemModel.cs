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

        BasePastedContentTypeDataModel ContentType { get; set; }

        Task OpenFile();

        Task OpenContainingFolder();

        Task OpenContainingFolder(bool checkForReference);

        /// <summary>
        /// Updates <see cref="Item"/> with new <paramref name="item"/> and also updates <see cref="SourceItem"/>
        /// <br/><br/>
        /// Note:
        /// <br/>
        /// This function is considered as *dangerous* since calling it may yield unexpected results
        /// </summary>
        /// <param name="item">New item to replace the old one with</param>
        void DangerousUpdateFile(IStorageItem item);
    }
}
