using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public partial class VideoCanvasViewModel : BaseCanvasViewModel
    {
        //[ObservableProperty] private double _Volume;
        //[ObservableProperty] private bool _IsPlaying;
        [ObservableProperty] private bool _IsLooping;
        //[ObservableProperty] private TimeSpan _TotalTime;
        //[ObservableProperty] private TimeSpan _CurrentTime;
        [ObservableProperty] private IMediaSource? _MediaSource;

        private IMediaService MediaService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        /// <inheritdoc/>
        public override IStorable? Storable { get; }

        public VideoCanvasViewModel(IMediaSource mediaSource, IDataSourceModel sourceModel)
            : base(sourceModel)
        {
            _MediaSource = mediaSource;
        }

        public VideoCanvasViewModel(IFile videoFile, IDataSourceModel sourceModel)
            : base(sourceModel)
        {
            Storable = videoFile;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Storable is not IFile file)
                return;

            MediaSource?.Dispose();
            MediaSource = await MediaService.GetVideoPlaybackAsync(file, cancellationToken);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            MediaSource?.Dispose();
        }
    }
}
