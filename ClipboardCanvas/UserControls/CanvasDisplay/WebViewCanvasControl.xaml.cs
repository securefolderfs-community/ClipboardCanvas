using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.ModelViews;

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

        private void ContentWebView_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ControlView = this;
            this.ViewModel.NotifyWebViewLoaded();
        }

        public void NavigateToHtml(string html)
        {
            // TODO: Regression
            //this.ContentWebView.NavigateToString(html);
        }

        public void NavigateToSource(string source)
        {
            // TODO: Regression
            //this.ContentWebView.Source = new Uri(source);
        }

        #region IDisposable

        public void Dispose()
        {
            // TODO: Dispose WebView
        }

        #endregion
    }
}
