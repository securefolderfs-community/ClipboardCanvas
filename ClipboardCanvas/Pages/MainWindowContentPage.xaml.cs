using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindowContentPage : Page, INotifyPropertyChanged
    {
        private bool _IntroductionPanelLoad;
        public bool IntroductionPanelLoad
        {
            get => _IntroductionPanelLoad;
            set
            {
                if (_IntroductionPanelLoad != value)
                {
                    _IntroductionPanelLoad = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowContentPage()
        {
            this.InitializeComponent();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
