using System;
using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using Windows.Storage;
using System.IO;
using Microsoft.Web.WebView2.Core;
using System.Web;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class MediaCanvasControl : UserControl, IMediaCanvasControlView
    {
        private StorageFile _htmlTempFile;

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

        public async Task LoadFromMedia(IStorageFile file)
        {
            ContentWebView.Source = await CreateUriToHtmlFileForDisplaying(file);
        }

        public async Task LoadFromAudio(IStorageFile file)
        {
            ContentWebView.Source = await CreateUriToHtmlFileForDisplaying(file);
        }

        private async Task<Uri> CreateUriToHtmlFileForDisplaying(IStorageFile file)
        {
            StorageFolder htmlTempFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(Constants.LocalSettings.HTML_TEMPDATA_FOLDERNAME, CreationCollisionOption.OpenIfExists);

            _htmlTempFile = await htmlTempFolder.CreateFileAsync("mediahtml.html", CreationCollisionOption.ReplaceExisting);

            Uri uri = new Uri(file.Path);
            string encodedPath = uri.AbsoluteUri;
            string html = $"<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"></head><body style=\"background-color: #000000; margin: 0px; padding: 0px;\"><div style=\"height: 100vh; overflow: hidden;\"><video style=\"object-fit: contain; max-height: 100%; width: 100%;\" controls autoplay loop><source src=\"{encodedPath}\" type=\"video/mp4\"></video></div></body></html>";

            await FileIO.WriteTextAsync(_htmlTempFile, html);

            return new Uri(_htmlTempFile.Path);
        }

        public void Dispose()
        {
            this.ContentWebView.Close();
        }
    }
}
