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
    }
}
