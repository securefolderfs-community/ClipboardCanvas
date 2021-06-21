namespace ClipboardCanvas
{
    public static class Constants
    {
        public static class LocalSettings
        {
            public const string SETTINGS_FOLDERNAME = "app_settings";

            public const string USER_SETTINGS_FILENAME = "user_settings.json";

            public const string COLLECTION_LOCATIONS_FILENAME = "saved_collections.json";

            public const string CANVAS_SETTINGS_FILENAME = "canvas_settings.json";

            public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";
        }

        public static class UI
        {
            public static class CanvasContent
            {
                public const long FALLBACK_TEXTLOAD_MAX_FILESIZE = 1048576L;

                public const int SHOW_LOADING_RING_DELAY = 100;

                public const int COLLECTION_RELOADING_TIP_DELAY = 400;

                public const int FILE_PASTING_TIP_DELAY = 150;
            }

            public static class Notifications
            {
                public const int NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL = 50;
            }
        }

        public static class FileSystem
        {
            public const string REFERENCE_FILELIST_EXTENSION = ".ccvrx";

            public const string REFERENCE_FILE_EXTENSION = ".ccvr";

            public const string WEBSITE_LINK_FILE_EXTENSION = ".ccvwl";

            public const int COPY_FILE_BUFFER_SIZE = 1024 * 1024; // 1mb
        }

        public static class Collections
        {
            public const string DEFAULT_COLLECTION_TOKEN = "$DEFAULT_COLLECTION$";

            public const int DOUBLE_CLICK_DELAY_MILLISECONDS = 300;
        }

        public static class Debugging
        {
            public const bool FIRST_CHANCE_EXCEPTION_DEBUGGING = false;
        }
    }
}
