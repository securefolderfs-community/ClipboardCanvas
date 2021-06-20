using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;

namespace ClipboardCanvas.Services
{
    public class DefaultDialogService : IDialogService
    {
        #region IDialogService

        public IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel)
        {
            IDialog<TViewModel> dialog;

            Type dialogType = GetDialogType(viewModel);

            if (dialogType == null)
            {
                throw new TypeLoadException($"The dialog for {viewModel} couldn't be found.");
            }

            dialog = GetDialogFromType<TViewModel, IDialog<TViewModel>>(dialogType, viewModel);
            dialog.ViewModel = viewModel;

            return dialog;
        }

        public async Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel)
        {
            IDialog<TViewModel> dialog = GetDialog(viewModel);

            return await dialog.ShowAsync();
        }

        public IInAppNotification GetNotification(InAppNotificationControlViewModel viewModel = null)
        {
            IInAppNotification notification = MainPage.Instance.MainInAppNotification;

            if (viewModel != null)
            {
                notification.ViewModel = viewModel;
            }

            return notification;
        }

        public IInAppNotification ShowNotification(InAppNotificationControlViewModel viewModel = null, int miliseconds = 0)
        {
            IInAppNotification notification = GetNotification(viewModel);

            notification.Show(miliseconds);
            return notification;
        }

        public async Task<StorageFolder> PickSingleFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            return folder;
        }

        #endregion

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

        private TRequestedDialog GetDialogFromType<TViewModel, TRequestedDialog>(Type dialogType, TViewModel viewModel)
        {
            object dialogInstance = Activator.CreateInstance(dialogType);

            if (dialogInstance is not TRequestedDialog dialog)
            {
                throw new ArgumentException($"Dialog of type {dialogType} does not implement {nameof(IDialog<TViewModel>)}.");
            }

            return dialog;
        }
    }
}
