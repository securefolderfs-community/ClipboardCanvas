using ClipboardCanvas.Shared.ComponentModel;
using System.ComponentModel;

namespace ClipboardCanvas.Sdk.Services.Settings
{
    /// <summary>
    /// An interface to manage user preferences and settings.
    /// </summary>
    public interface IUserSettings : IPersistable, INotifyPropertyChanged
    {
    }
}
