using System.Threading.Tasks;
using Windows.Storage;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.Models
{
    /// <summary>
    /// This interface is an item model that's within <see cref="ICollectionModel"/>
    /// </summary>
    public interface ICollectionItemModel
    {
        StorageFile File { get; }

        BasePastedContentTypeDataModel ContentType { get; set; }

        Task OpenFile();

        Task OpenContainingFolder();

        Task OpenContainingFolder(bool checkForReference);

        /// <summary>
        /// Updates <see cref="File"/> with new <paramref name="file"/>
        /// <br/><br/>
        /// Note:
        /// <br/>
        /// This function is considered as *dangerous* since calling it may yield unexpected results
        /// </summary>
        /// <param name="file">New file to replace the old one with</param>
        void DangerousUpdateFile(StorageFile file);
    }
}
