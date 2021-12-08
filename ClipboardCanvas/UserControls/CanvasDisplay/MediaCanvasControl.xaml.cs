using System;
using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class MediaCanvasControl : UserControl, IMediaCanvasControlView // TODO: Regression
    {
        public MediaCanvasViewModel ViewModel
        {
            get => (MediaCanvasViewModel)DataContext;
        }

        public TimeSpan Position
        {
            get => TimeSpan.FromMilliseconds(0);// MediaPlayerContent.MediaPlayer.PlaybackSession.Position;
            set { }// MediaPlayerContent.MediaPlayer.PlaybackSession.Position = value;
        }

        public bool IsLoopingEnabled
        {
            get => false;// MediaPlayerContent.MediaPlayer.IsLoopingEnabled;
            set { }// MediaPlayerContent.MediaPlayer.IsLoopingEnabled = value;
        }

        public double Volume
        {
            get => 0.0d;//MediaPlayerContent.MediaPlayer.Volume;
            set { }// MediaPlayerContent.MediaPlayer.Volume = value;
        }

        public MediaCanvasControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.ControlView = this;
            this.ViewModel.UpdateMediaControl();
        }
    }
}
