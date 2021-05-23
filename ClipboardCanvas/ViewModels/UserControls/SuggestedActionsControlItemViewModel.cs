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
    public class SuggestedActionsControlItemViewModel : ObservableObject, IDisposable, IEquatable<SuggestedActionsControlItemViewModel>
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

        private BitmapImage _IconImage;
        public BitmapImage IconImage
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

        public SuggestedActionsControlItemViewModel(Action executeAction, string displayText, string glyphIcon)
            : this(executeAction, displayText)
        {
            this.GlyphIcon = glyphIcon;
            this.GlyphIconVisibility = Visibility.Visible;
            this.IconImageVisibility = Visibility.Collapsed;
        }

        public SuggestedActionsControlItemViewModel(Action executeAction, string displayText, BitmapImage iconImage)
            : this(executeAction, displayText)
        {
            this.IconImage = iconImage;
            this.GlyphIconVisibility = Visibility.Collapsed;
            this.IconImageVisibility = Visibility.Visible;
        }

        private SuggestedActionsControlItemViewModel(Action executeAction, string displayText)
        {
            this.DisplayText = displayText;
            this.ExecuteAction = executeAction;
        }

        #endregion

        #region IEquatable

        public bool Equals(SuggestedActionsControlItemViewModel other)
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
