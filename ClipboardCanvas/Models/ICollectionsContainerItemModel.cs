using System.Threading.Tasks;
using Windows.Storage;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.Models
{
    /// <summary>
    /// This interface is an item model that's within <see cref="ICollectionsContainerModel"/>
    /// </summary>
    public interface ICollectionsContainerItemModel
    {
        StorageFile File { get; }

        BasePastedContentTypeDataModel ContentType { get; set; }

        Task OpenFile();

        Task OpenContainingFolder();

        /// <summary>
        /// Updates <see cref="File"/> with new <paramref name="file"/>
        /// <br/><br/>
        /// Note:
        /// <br/>
        /// This function is considered as *dangerous* since calling it may yield unexpected results
        /// </summary>
        /// <param name="file">New file</param>
        void DangerousUpdateFile(StorageFile file);
    }
}
