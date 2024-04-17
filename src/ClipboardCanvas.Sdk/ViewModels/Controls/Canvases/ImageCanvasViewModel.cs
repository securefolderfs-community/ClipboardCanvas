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

        private IImageService ImageService { get; } = Ioc.Default.GetRequiredService<IImageService>();

        /// <inheritdoc/>
        public override IStorable? Storable { get; }

        public ImageCanvasViewModel(IImage image, IDataSourceModel collectionModel)
            : base(collectionModel)
        {
            Image = image;
        }

        public ImageCanvasViewModel(IFile imageFile, IDataSourceModel collectionModel)
            : base(collectionModel)
        {
            Storable = imageFile;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Storable is not IFile file)
                return;

            Image = await ImageService.ReadImageAsync(file, cancellationToken);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Image?.Dispose();
        }
    }
}
