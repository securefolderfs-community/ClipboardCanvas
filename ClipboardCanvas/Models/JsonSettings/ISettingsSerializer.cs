namespace ClipboardCanvas.Models.JsonSettings
{
    public interface ISettingsSerializer
    {
        bool WriteToFile(string json);

        string ReadFromFile();
    }
}
