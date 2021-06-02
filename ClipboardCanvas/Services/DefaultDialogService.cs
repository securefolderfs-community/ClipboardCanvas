using ClipboardCanvas.Dialogs;
using ClipboardCanvas.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.Services
{
    public class DefaultDialogService : IDialogService
    {
        public async Task<StorageFolder> PickSingleFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            return folder;
        }

        public IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel)
        {
            IDialog<TViewModel> dialog;

            Type dialogType = GetDialogType(viewModel);

            if (dialogType == null)
            {
                throw new TypeLoadException($"The dialog for {viewModel} couldn't be found.");
            }

            dialog = GetDialogFromType(dialogType, viewModel);
            dialog.ViewModel = viewModel;

            return dialog;
        }

        public async Task<ContentDialogResult> ShowDialog<TViewModel>(TViewModel viewModel)
        {
            IDialog<TViewModel> dialog = GetDialog(viewModel);

            return await dialog.ShowAsync();
        }

        private Type GetDialogType<TViewModel>(TViewModel viewModel)
        {
            Type viewModelType = viewModel.GetType();
            string dialogName = GetDialogName(viewModelType);

            Type dialogType = viewModelType.GetTypeInfo().Assembly.GetType(dialogName);
            return dialogType;
        }

        private string GetDialogName(Type viewModelType)
        {
            if (viewModelType.FullName != null)
            {
                string dialogName = viewModelType.FullName.Replace("ViewModels.", string.Empty);

                if (dialogName.EndsWith("ViewModel", StringComparison.Ordinal))
                {
                    return dialogName.Substring(0, dialogName.Length - "ViewModel".Length);
                }
            }

            throw new TypeLoadException($"The {viewModelType} isn't suffixed with \"ViewModel\".");
        }

        private IDialog<TViewModel> GetDialogFromType<TViewModel>(Type dialogType, TViewModel viewModel)
        {
            object dialogInstance = Activator.CreateInstance(dialogType);

            if (dialogInstance is not IDialog<TViewModel> contentDialog)
            {
                throw new ArgumentException($"Dialog of type {dialogType} does not implement {nameof(IDialog<TViewModel>)}.");
            }

            return contentDialog;
        }
    }
}
