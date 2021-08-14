using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Models;
using System;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class WebViewCanvasControl : UserControl, IWebViewCanvasControlView
    {
        public WebViewCanvasViewModel ViewModel
        {
            get => (WebViewCanvasViewModel)DataContext;
        }

        public WebViewCanvasControl()
        {
            this.InitializeComponent();
        }

        private void ContentWebView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.ControlView = this;
            this.ViewModel.NotifyWebViewLoaded();
        }

        public void NavigateToHtml(string html)
        {
            this.ContentWebView.NavigateToString(html);
        }

        public void NavigateToSource(string source)
        {
            this.ContentWebView.Source = new Uri(source);
        }

        #region IDisposable

        public void Dispose()
        {
            // TODO: Dispose WebView
        }

        #endregion
    }
}
