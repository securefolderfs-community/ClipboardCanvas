namespace ClipboardCanvas
{
    public static class Constants
    {
        public static class LocalSettings
        {
            public const string SETTINGS_FOLDERNAME = "app_settings";

            public const string USER_SETTINGS_FILENAME = "user_settings.json";

            public const string COLLECTION_LOCATIONS_FILENAME = "saved_collections.json";
        }

        public static class CanvasContent
        {
            public const long FALLBACK_TEXTLOAD_MAX_FILESIZE = 1048576L;
        }

        public static class Collections
        {
            public const string DEFAULT_COLLECTION_ID = "$DEFAULT_COLLECTION$";
        }
    }
}
