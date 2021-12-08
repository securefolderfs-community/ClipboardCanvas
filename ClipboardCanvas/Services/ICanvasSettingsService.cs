namespace ClipboardCanvas.Services
{
    public interface ICanvasSettingsService
    {
        bool MediaCanvas_IsLoopingEnabled { get; set; }

        double MediaCanvas_UniversalVolume { get; set; }
    }
}
