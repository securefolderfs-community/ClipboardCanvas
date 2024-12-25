using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Browser
{
    public abstract partial class BrowserItemViewModel : ObservableObject, IWrapper<IStorable>, IViewable, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <summary>
        /// Gets the item associated with this view model.
        /// </summary>
        public virtual IStorable Inner { get; }

        protected BrowserItemViewModel(IStorable storable)
        {
            Inner = storable;
            Title = storable.Name;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
