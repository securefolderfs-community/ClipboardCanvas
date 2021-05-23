using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.AttachedProperties
{
    public class WebViewNavigationAttachedProperty : BaseAttachedProperty<WebViewNavigationAttachedProperty, string, WebView>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not WebView webView || e.NewValue is not string html)
            {
                return;
            }

            webView.NavigateToString(html);
        }
    }
}
