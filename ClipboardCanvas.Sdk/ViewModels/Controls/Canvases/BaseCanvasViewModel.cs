using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public abstract class BaseCanvasViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        protected ICanvasSourceModel CollectionModel { get; }

        protected BaseCanvasViewModel(ICanvasSourceModel collectionModel)
        {
            CollectionModel = collectionModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
