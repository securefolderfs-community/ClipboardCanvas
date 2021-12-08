namespace ClipboardCanvas.Services
{
    public interface IApplicationSettingsService
    {
        string LastVersionNumber { get; set; }

        bool IsUserIntroduced { get; set; }
    }
}
