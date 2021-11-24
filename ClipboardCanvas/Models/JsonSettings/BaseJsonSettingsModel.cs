using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Models.JsonSettings.Implementation;
using ClipboardCanvas.UnsafeNative;
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.Models.JsonSettings
{
    /// <summary>
    /// Clipboard Canvas
    /// A base class to easily manage all application's settings.
    /// </summary>
    public abstract class BaseJsonSettingsModel : ISettingsSharingContext
    {
        #region Protected Members

        protected int registeredMembers = 0;

        protected ISettingsSharingContext settingsSharingContext;

        public IJsonSettingsSerializer JsonSettingsSerializer { get; set; }

        public ISettingsSerializer SettingsSerializer { get; set; }

        #endregion Protected Members

        #region Properties

        private string _FilePath;
        public string FilePath
        {
            get => settingsSharingContext?.FilePath ?? _FilePath;
            protected set => _FilePath = value;
        }

        private IJsonSettingsDatabase _JsonSettingsDatabase;
        public IJsonSettingsDatabase JsonSettingsDatabase
        {
            get => settingsSharingContext?.JsonSettingsDatabase ?? _JsonSettingsDatabase;
            protected set => _JsonSettingsDatabase = value;
        }

        #endregion Properties

        #region Events

        public event EventHandler<SettingChangedEventArgs> OnSettingChangedEvent;

        #endregion Events

        #region Constructor

        public BaseJsonSettingsModel()
        {
        }

        public BaseJsonSettingsModel(string filePath)
            : this(filePath, null, null, null)
        {
        }

        public BaseJsonSettingsModel(ISettingsSharingContext settingsSharingContext)
        {
            RegisterSettingsContext(settingsSharingContext);
            Initialize();
        }

        public BaseJsonSettingsModel(string filePath, bool isCachingEnabled,
            IJsonSettingsSerializer jsonSettingsSerializer = null,
            ISettingsSerializer settingsSerializer = null)
        {
            this.FilePath = filePath;
            Initialize();

            this.JsonSettingsSerializer = jsonSettingsSerializer;
            this.SettingsSerializer = settingsSerializer;

            // Fallback
            this.JsonSettingsSerializer ??= new DefaultJsonSettingsSerializer();
            this.SettingsSerializer ??= new DefaultSettingsSerializer(this.FilePath);

            if (isCachingEnabled)
            {
                this.JsonSettingsDatabase = new CachingJsonSettingsDatabase(this.JsonSettingsSerializer, this.SettingsSerializer);
            }
            else
            {
                this.JsonSettingsDatabase = new DefaultJsonSettingsDatabase(this.JsonSettingsSerializer, this.SettingsSerializer);
            }
        }

        public BaseJsonSettingsModel(string filePath,
            IJsonSettingsSerializer jsonSettingsSerializer,
            ISettingsSerializer settingsSerializer,
            IJsonSettingsDatabase jsonSettingsDatabase)
        {
            this.FilePath = filePath;
            Initialize();

            this.JsonSettingsSerializer = jsonSettingsSerializer;
            this.SettingsSerializer = settingsSerializer;
            this.JsonSettingsDatabase = jsonSettingsDatabase;

            // Fallback
            this.JsonSettingsSerializer ??= new DefaultJsonSettingsSerializer();
            this.SettingsSerializer ??= new DefaultSettingsSerializer(this.FilePath);
            this.JsonSettingsDatabase ??= new DefaultJsonSettingsDatabase(this.JsonSettingsSerializer, this.SettingsSerializer);
        }

        #endregion Constructor

        #region Helpers

        protected virtual void Initialize()
        {
            // Create the file
            UnsafeNativeApis.CreateDirectoryFromApp(Path.GetDirectoryName(FilePath), IntPtr.Zero);
            UnsafeNativeHelpers.CreateFileForWrite(FilePath, false).CloseFileHandle();
        }

        public virtual bool FlushSettings()
        {
            return JsonSettingsDatabase.FlushSettings();
        }

        public virtual object ExportSettings()
        {
            return JsonSettingsDatabase.ExportSettings();
        }

        public virtual bool ImportSettings(object import)
        {
            return JsonSettingsDatabase.ImportSettings(import);
        }

        public bool RegisterSettingsContext(ISettingsSharingContext settingsSharingContext)
        {
            if (this.settingsSharingContext == null)
            {
                // Can set only once
                this.settingsSharingContext = settingsSharingContext;
                return true;
            }

            return false;
        }

        public ISettingsSharingContext GetSharingContext()
        {
            registeredMembers++;
            return settingsSharingContext ?? this;
        }

        public bool GuaranteeAddToList<T>(IList<T> list, T item, string listSettingName)
        {
            list?.Add(item);

            bool result = Set(list, listSettingName);
            return FlushSettings() && result;
        }

        public bool GuaranteeAddRangeToList<T>(List<T> list, IEnumerable<T> items, string listSettingName)
        {
            list?.AddRange(items);

            bool result = Set(list, listSettingName);
            return FlushSettings() && result;
        }

        public bool GuaranteeRemoveFromList<T>(IList<T> list, T item, string listSettingName)
        {
            if (!list?.Remove(item) ?? true)
            {
                return false;
            }

            bool result = Set(list, listSettingName);
            return FlushSettings() && result;
        }

        public virtual void RaiseOnSettingChangedEvent(object sender, SettingChangedEventArgs e)
        {
            if (settingsSharingContext != null)
            {
                settingsSharingContext.RaiseOnSettingChangedEvent(sender, e);
            }
            else
            {
                OnSettingChangedEvent?.Invoke(sender, e);
            }
        }

        #endregion Helpers

        #region Get, Set

        protected virtual TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return defaultValue;
            }

            return JsonSettingsDatabase.GetValue<TValue>(propertyName, defaultValue);
        }

        protected virtual bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            if (JsonSettingsDatabase.UpdateKey(propertyName, value))
            {
                RaiseOnSettingChangedEvent(this, new SettingChangedEventArgs(propertyName, value));
                return true;
            }

            return false;
        }

        #endregion Get, Set
    }
}
