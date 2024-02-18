using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage;
using System;

namespace ClipboardCanvas.Sdk.Models
{
    /// <summary>
    /// Represents a collection source where canvases reside.
    /// </summary>
    public interface ICanvasSourceModel : IModifiableFolder, IAsyncInitialize, IAsyncDisposable, IDisposable
    {
    }
}
