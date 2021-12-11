using System;
using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using Windows.Storage;
using System.IO;
using Microsoft.Web.WebView2.Core;

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

        private async void ContentWebView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ContentWebView.EnsureCoreWebView2Async(); // Init

            await this.ViewModel.NotifyWebViewLoaded();
        }

        public void LoadFromMedia(IStorageFile file)
        {
            ContentWebView.Source = new Uri(file.Path);
        }

        public void LoadFromAudio(IStorageFile file)
        {
            ContentWebView.Source = new Uri(file.Path);
        }

        public void Dispose()
        {
            this.ContentWebView.Close();
        }
    }
}
