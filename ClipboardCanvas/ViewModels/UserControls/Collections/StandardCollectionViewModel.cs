using System;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.Storage;

using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Interfaces.Collections;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public class StandardCollectionViewModel : BaseCollectionViewModel, ICollectionNameEditable, ICollectionRemovable
    {
        #region Events

        public event EventHandler<CheckRenameCollectionRequestedEventArgs> OnCheckRenameCollectionRequestedEvent;

        public event EventHandler<RemoveCollectionRequestedEventArgs> OnRemoveCollectionRequestedEvent;

        #endregion

        #region Constructor

        public StandardCollectionViewModel(StorageFolder collectionFolder)
            : this(collectionFolder, null)
        {
        }

        public StandardCollectionViewModel(string collectionPath)
            : this(null, collectionPath)
        {
        }

        public StandardCollectionViewModel(StorageFolder collectionFolder, string collectionPath)
            : base(collectionFolder, collectionPath)
        {
            // Create commands
            StartRenameCollectionCommand = new RelayCommand(StartRename);
            RenameBoxKeyDownCommand = new AsyncRelayCommand<KeyRoutedEventArgs>(RenameBoxKeyDown);
            RenameBoxLostFocusCommand = new AsyncRelayCommand<RoutedEventArgs>(RenameBoxLostFocus);
            RemoveCollectionCommand = new RelayCommand(RemoveCollection);
        }

        #endregion

        #region Command Implementation

        private async Task RenameBoxKeyDown(KeyRoutedEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case VirtualKey.Enter:
                    {
                        await ConfirmRename();

                        break;
                    }

                case VirtualKey.Escape:
                    {
                        CancelRename();

                        break;
                    }
            }
        }

        private async Task RenameBoxLostFocus(RoutedEventArgs e)
        {
            if (!IsEditingName) // Don't fire when textbox is being hidden
            {
                return;
            }

            await ConfirmRename();
        }

        #endregion

        #region Override

        public override bool CheckCollectionAvailability()
        {
            if (App.IsInRestrictedAccessMode)
            {
                SetCollectionError(RestrictedAccessUnauthorized);
                return false;
            }
            else if (!StorageHelpers.Exists(CollectionPath))
            {
                SetCollectionError(CollectionFolderNotFound);
                return false;
            }

            SetCollectionError(SafeWrapperResult.SUCCESS);
            return true;
        }

        #endregion

        #region ICollectionNameEditable

        public void CancelRename()
        {
            // Cancel
            IsEditingName = false;
        }

        public async Task<bool> ConfirmRename()
        {
            string newName = EditBoxText;
            isEditingName = false;

            CheckRenameCollectionRequestedEventArgs args = new CheckRenameCollectionRequestedEventArgs(this, newName);
            OnCheckRenameCollectionRequestedEvent?.Invoke(this, args);

            if (args.canRename)
            {
                SafeWrapperResult result = await FilesystemOperations.RenameItem(collectionFolder, newName, NameCollisionOption.FailIfExists);

                if (result)
                {
                    CollectionPath = collectionFolder.Path;

                    OnPropertyChanged(nameof(DisplayName));
                    OnPropertyChanged(nameof(IsEditingName));

                    // Also update settings
                    SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
                    SettingsSerializationHelpers.UpdateLastSelectedCollectionSetting(this);

                    return true;
                }
                else
                {
                    // Post a notification informing that rename had failed
                    PushErrorNotification("Couldn't rename the collection", result);
                }
            }

            OnPropertyChanged(nameof(IsEditingName));
            return false;
        }

        public void StartRename()
        {
            if (IsCollectionAvailable)
            {
                EditBoxText = DisplayName;
                IsEditingName = true;

                // Focus EditBox
                EditBoxFocus = true;
                EditBoxFocus = false;
            }
        }

        #endregion

        #region ICollectionRemovable

        public void RemoveCollection()
        {
            OnRemoveCollectionRequestedEvent?.Invoke(this, new RemoveCollectionRequestedEventArgs(this));
        }

        #endregion
    }
}
