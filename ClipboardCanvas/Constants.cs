namespace ClipboardCanvas
{
    public static class Constants
    {
        public static class ClipboardCanvasRepository
        {
            public const string REPOSITORY_OWNER = "d2dyno1";

            public const string REPOSITORY_NAME = "ClipboardCanvas";

            public const string PRIVACY_POLICY_FILENAME = "Privacy.md";
        }

        public static class LocalSettings
        {
            public const string SETTINGS_FOLDERNAME = "app_settings";

            public const string USER_SETTINGS_FILENAME = "user_settings.json";

            public const string COLLECTION_SETTINGS_FILENAME = "collections_settings.json";

            public const string TIMELINE_SETTINGS_FILENAME = "timeline_settings.json";

            public const string CANVAS_SETTINGS_FILENAME = "canvas_settings.json";

            public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";
        }

        public static class UI
        {
            public const int CONTROL_LOAD_DELAY = 10;

            public static class CanvasContent
            {
                public const long FALLBACK_TEXTLOAD_MAX_FILESIZE = 1048576L;

                public const int SHOW_LOADING_RING_DELAY = 100;

                public const int COLLECTION_RELOADING_TIP_DELAY = 400;

                public const int FILE_PASTING_TIP_DELAY = 150;

                public const uint SIMPLE_CANVAS_THUMBNAIL_SIZE = 128u;

                public const int INFINITE_CANVAS_SAVE_INTERVAL = 1000;

                public const int INFINITE_CANVAS_ERROR_SHOW_TIME = 2000;

                public const string INFINITE_CANVAS_DRAGGED_OBJECT_ID = "clipboard_canvas__dragged_control_object";
            }

            public static class Notifications
            {
                public const int NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL = 50;

                public const int NOTIFICATION_DEFAULT_SHOW_TIME = 4000;
            }

            public static class Animations
            {
                public const string CONNECTED_ANIMATION_COLLECTION_PREVIEW_ITEM_OPEN_REQUESTED_TOKEN = "collection_preview_item_opening_connected_animation";
            }

            public static class Timeline
            {
                public const int MAX_ITEMS_PER_SECTION = 10;

                public const int MAX_SECTIONS = 10;
            }
        }

        public static class FileSystem
        {
            public const string EXCEPTIONLOG_FILENAME = "clipboardcanvas_exceptionlog.log";

            public const string COLLECTION_ICONS_FOLDERNAME = "collections_icons";

            public const string REFERENCE_FILE_EXTENSION = ".ccvr";

            public const string WEBSITE_LINK_FILE_EXTENSION = ".ccvwl";

            public const string INFINITE_CANVAS_EXTENSION = ".ccvic";

            public const string INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION = ".ccviccfg";

            public const string INFINITE_CANVAS_CONFIGURATION_FILENAME = "infinite_canvas_configuration.ccviccfg";

            public const string INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME = "infinite_canvas_preview.png";

            public const string CANVAS_FILE_FILENAME_DATE_FORMAT = "dd.MM.yyyy HH_mm_ss";

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
