using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public abstract partial class BaseCanvasViewModel : ObservableObject, IAsyncInitialize, IViewable, IDisposable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsImmersed;

        public IDataSourceModel SourceModel { get; }

        public abstract IStorable? Storable { get; }

        protected BaseCanvasViewModel(IDataSourceModel sourceModel)
        {
            SourceModel = sourceModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
