using ClipboardCanvas.Extensions;
using ClipboardCanvas.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.UserControls.StatusCenter
{
    public class StatusCenterViewModel : ObservableObject
    {
        private IStatusCenterService StatusCenterService { get; } = Ioc.Default.GetService<IStatusCenterService>();

        public ObservableCollection<StatusCenterItemViewModel> Items { get; private set; } = new ObservableCollection<StatusCenterItemViewModel>();

        private bool _NoItemsTextLoad = true;
        public bool NoItemsTextLoad
        {
            get => _NoItemsTextLoad;
            set => SetProperty(ref _NoItemsTextLoad, value);
        }

        public void AddItem(StatusCenterItemViewModel itemToAdd)
        {
            AddItem(itemToAdd, TimeSpan.Zero);
        }

        public async void AddItem(StatusCenterItemViewModel itemToAdd, TimeSpan appendDelay)
        {
            if (appendDelay != TimeSpan.Zero)
            {
                await Task.Delay(appendDelay);
            }

            if (!itemToAdd.IsOperationFinished)
            {
                NoItemsTextLoad = false;
                StatusCenterService.ShowStatusCenter();
                Items.Add(itemToAdd);
            }
        }

        public void RemoveItem(StatusCenterItemViewModel banner)
        {
            Items.Remove(banner);

            if (Items.IsEmpty())
            {
                NoItemsTextLoad = true;
                StatusCenterService.HideStatusCenter();
            }
        }
    }
}
