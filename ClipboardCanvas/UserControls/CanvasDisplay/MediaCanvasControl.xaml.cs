using System;
using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class MediaCanvasControl : UserControl, IMediaCanvasControlView
    {
        public MediaCanvasViewModel ViewModel
        {
            get => (MediaCanvasViewModel)DataContext;
        }

        public TimeSpan Position
        {
            get => MediaPlayerContent.MediaPlayer.PlaybackSession.Position;
            set => MediaPlayerContent.MediaPlayer.PlaybackSession.Position = value;
        }

        public bool IsLoopingEnabled
        {
            get => MediaPlayerContent.MediaPlayer.IsLoopingEnabled;
            set => MediaPlayerContent.MediaPlayer.IsLoopingEnabled = value;
        }

        public double Volume
        {
            get => MediaPlayerContent.MediaPlayer.Volume;
            set => MediaPlayerContent.MediaPlayer.Volume = value;
        }

        public MediaCanvasControl()
        {
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.ControlView = this;
            this.ViewModel.UpdateMediaControl();
        }
    }
}
