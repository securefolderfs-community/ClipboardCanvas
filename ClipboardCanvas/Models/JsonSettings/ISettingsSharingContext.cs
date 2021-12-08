using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.Models.JsonSettings
{
    public interface ISettingsSharingContext
    {
        string FilePath { get; }

        IJsonSettingsDatabase JsonSettingsDatabase { get; }

        ISettingsSharingContext GetSharingContext();

        void RaiseOnSettingChangedEvent(object sender, SettingChangedEventArgs e);
    }
}
