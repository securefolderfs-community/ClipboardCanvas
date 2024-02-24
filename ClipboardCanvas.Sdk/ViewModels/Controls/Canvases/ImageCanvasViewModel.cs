using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public sealed partial class ImageCanvasViewModel : BaseCanvasViewModel
    {
        [ObservableProperty] private IImage? _Image;

        public ImageCanvasViewModel(ICanvasSourceModel collectionModel)
            : base(collectionModel)
        {
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
