using ClipboardCanvas.Shared.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ClipboardCanvas.Sdk.Models
{
    /// <summary>
    /// Manages the list of user-saved collections.
    /// </summary>
    public interface ICollectionStoreModel : ICollection<ICanvasSourceModel>, INotifyCollectionChanged, IPersistable
    {
    }
}
