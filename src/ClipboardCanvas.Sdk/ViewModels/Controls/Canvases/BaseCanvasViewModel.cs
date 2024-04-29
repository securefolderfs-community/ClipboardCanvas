using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public abstract partial class BaseCanvasViewModel : ObservableObject, IAsyncInitialize, IViewable, IDisposable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsImmersed;
        [ObservableProperty] private ObservableCollection<ActionViewModel>? _PrimaryActions;
        [ObservableProperty] private ObservableCollection<ActionViewModel>? _SecondaryActions;

        public IDataSourceModel SourceModel { get; }

        public virtual IStorable? Storable { get; }

        protected BaseCanvasViewModel(IStorable storable, IDataSourceModel sourceModel)
            : this(sourceModel)
        {
            Storable = storable;
            Title = Path.GetFileName(storable.Id);
        }

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
