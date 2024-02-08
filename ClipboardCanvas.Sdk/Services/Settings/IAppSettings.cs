using ClipboardCanvas.Shared.ComponentModel;
using System.ComponentModel;

namespace ClipboardCanvas.Sdk.Services.Settings
{
    /// <summary>
    /// An interface for storing application configuration and settings.
    /// </summary>
    public interface IAppSettings : IPersistable, INotifyPropertyChanged
    {
    }
}
