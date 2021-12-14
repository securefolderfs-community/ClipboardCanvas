using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class SuggestedActionsControlItemViewModel : ObservableObject, IDisposable, IEquatable<SuggestedActionsControlItemViewModel>
    {
        #region Public Properties

        public ICommand ExecuteCommand { get; private set; }

        private string _DisplayText;
        public string DisplayText
        {
            get => _DisplayText;
            private set => SetProperty(ref _DisplayText, value);
        }

        private bool _GlyphIconLoad;
        public bool GlyphIconLoad
        {
            get => _GlyphIconLoad;
            set => SetProperty(ref _GlyphIconLoad, value);
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

        private bool _IconImageLoad;
        public bool IconImageLoad
        {
            get => _IconImageLoad;
            set => SetProperty(ref _IconImageLoad, value);
        }

        #endregion

        #region Constructor

        // TODO: Pass ICommand and not action
        public SuggestedActionsControlItemViewModel(ICommand executeCommand, string displayText, string glyphIcon)
            : this(executeCommand, displayText)
        {
            this.GlyphIcon = glyphIcon;
            this.GlyphIconLoad = true;
            this.IconImageLoad = false;
        }

        public SuggestedActionsControlItemViewModel(ICommand executeCommand, string displayText, BitmapImage iconImage)
            : this(executeCommand, displayText)
        {
            this.IconImage = iconImage;
            this.GlyphIconLoad = false;
            this.IconImageLoad = true;
        }

        private SuggestedActionsControlItemViewModel(ICommand executeCommand, string displayText)
        {
            this.DisplayText = displayText;
            this.ExecuteCommand = executeCommand;
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
            ExecuteCommand = null;
            _IconImage = null;
        }

        #endregion
    }
}
