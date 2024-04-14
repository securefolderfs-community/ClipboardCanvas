using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage;
using System;

namespace ClipboardCanvas.Sdk.Models
{
    /// <summary>
    /// Represents a data source where canvases reside.
    /// </summary>
    public interface IDataSourceModel : IModifiableFolder, IAsyncInitialize, IDisposable
    {
    }
}
