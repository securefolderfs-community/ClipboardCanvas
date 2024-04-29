using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage;
using System;
using System.Collections.Specialized;

namespace ClipboardCanvas.Sdk.Models
{
    /// <summary>
    /// Represents a data source where canvases reside.
    /// </summary>
    public interface IDataSourceModel : INotifyCollectionChanged, IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets the source folder of the data.
        /// </summary>
        IFolder Source { get; }

        /// <summary>
        /// Gets the name of this source model.
        /// </summary>
        string Name { get; }
    }
}
