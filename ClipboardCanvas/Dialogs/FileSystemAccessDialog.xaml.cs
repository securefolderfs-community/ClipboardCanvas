using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.ViewModels.Dialogs;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Dialogs
{
    public sealed partial class FileSystemAccessDialog : ContentDialog, IDialog<FileSystemAccessDialogViewModel>
    {
        public FileSystemAccessDialogViewModel ViewModel
        {
            get => (FileSystemAccessDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public async new Task<DialogResult> ShowAsync() => (await base.ShowAsync()).ToDialogResult();

        public FileSystemAccessDialog()
        {
            this.InitializeComponent();
        }
    }
}
