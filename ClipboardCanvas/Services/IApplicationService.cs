namespace ClipboardCanvas.Services
{
    public interface IApplicationService
    {
        bool IsWindowActivated { get; }

        string AppVersion { get; }

        bool IsInRestrictedAccessMode { get; set; }
    }
}
