using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class AdaptiveOptionsControlItemViewModel : ObservableObject, IDisposable, IEquatable<AdaptiveOptionsControlItemViewModel>
    {
        #region Public Properties

        public Action ExecuteAction { get; private set; }

        private string _DisplayText;
        public string DisplayText
        {
            get => _DisplayText;
            private set => SetProperty(ref _DisplayText, value);
        }

        private Visibility _GlyphIconVisibility;
        public Visibility GlyphIconVisibility
        {
            get => _GlyphIconVisibility;
            set => SetProperty(ref _GlyphIconVisibility, value);
        }

        private string _GlyphIcon;
        public string GlyphIcon
        {
            get => _GlyphIcon;
            set => SetProperty(ref _GlyphIcon, value);
        }

        private ImageSource _IconImage;
        public ImageSource IconImage
        {
            get => _IconImage;
            set => SetProperty(ref _IconImage, value);
        }

        private Visibility _IconImageVisibility;
        public Visibility IconImageVisibility
        {
            get => _IconImageVisibility;
            set => SetProperty(ref _IconImageVisibility, value);
        }

        #endregion

        #region Constructor

        public AdaptiveOptionsControlItemViewModel(Action executeAction, string displayText, string glyphIcon)
            : this(executeAction, displayText)
        {
            this.GlyphIcon = glyphIcon;
            this.GlyphIconVisibility = Visibility.Visible;
            this.IconImageVisibility = Visibility.Collapsed;
        }

        public AdaptiveOptionsControlItemViewModel(Action executeAction, string displayText, BitmapImage iconImage)
            : this(executeAction, displayText)
        {
            this.IconImage = iconImage;
            this.GlyphIconVisibility = Visibility.Collapsed;
            this.IconImageVisibility = Visibility.Visible;
        }

        private AdaptiveOptionsControlItemViewModel(Action executeAction, string displayText)
        {
            this.DisplayText = displayText;
            this.ExecuteAction = executeAction;
        }

        #endregion

        #region IEquatable

        public bool Equals(AdaptiveOptionsControlItemViewModel other)
        {
            if (
                 this.DisplayText == other.DisplayText
                )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            ExecuteAction = null;
            _IconImage = null;
        }

        #endregion
    }
}
