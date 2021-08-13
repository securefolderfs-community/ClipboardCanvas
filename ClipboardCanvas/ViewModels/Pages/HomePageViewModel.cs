using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;

using ClipboardCanvas.Services;
using System;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class HomePageViewModel : ObservableObject, IDisposable
    {
        public ITimelineService TimelineService { get; } = Ioc.Default.GetService<ITimelineService>();

        public async Task LoadWidgets()
        {
            await TimelineService.LoadAllSectionsAsync();
        }

        #region IDisposable

        public void Dispose()
        {
            TimelineService.UnloadAllSections();
        }

        #endregion
    }
}
