using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Services;
using OwlCore.Storage;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public partial class ImageCanvasViewModel : BaseCanvasViewModel
    {
        [ObservableProperty] private IImage? _Image;
        protected IFile? imageFile;

        private IImageService ImageService { get; } = Ioc.Default.GetRequiredService<IImageService>();

        public ImageCanvasViewModel(IDataSourceModel collectionModel)
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
