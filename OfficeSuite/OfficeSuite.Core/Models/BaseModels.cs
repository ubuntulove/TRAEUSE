using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OfficeSuite.Core.Models
{
    public abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public virtual ModelBase Clone()
        {
            throw new NotImplementedException();
        }
    }

    public class DocumentInfo : ModelBase
    {
        private string _id;
        private string _name;
        private string _filePath;
        private DateTime _createdAt;
        private DateTime _modifiedAt;
        private DocumentType _documentType;
        private byte[] _thumbnail;
        private bool _isDirty;
        private Dictionary<string, object> _metadata;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public DateTime ModifiedAt
        {
            get => _modifiedAt;
            set => SetProperty(ref _modifiedAt, value);
        }

        public DocumentType DocumentType
        {
            get => _documentType;
            set => SetProperty(ref _documentType, value);
        }

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public Dictionary<string, object> Metadata
        {
            get => _metadata ??= new Dictionary<string, object>();
            set => SetProperty(ref _metadata, value);
        }

        public DocumentInfo()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
            _metadata = new Dictionary<string, object>();
        }

        public override ModelBase Clone()
        {
            return new DocumentInfo
            {
                Id = Id,
                Name = Name,
                FilePath = FilePath,
                CreatedAt = CreatedAt,
                ModifiedAt = ModifiedAt,
                DocumentType = DocumentType,
                Thumbnail = Thumbnail,
                IsDirty = IsDirty,
                Metadata = new Dictionary<string, object>(Metadata)
            };
        }
    }

    public enum DocumentType
    {
        Unknown = 0,
        Word = 1,
        Spreadsheet = 2,
        Presentation = 3
    }

    public class ApplicationSettings : ModelBase
    {
        private string _theme;
        private string _language;
        private bool _autoSave;
        private int _autoSaveInterval;
        private string _defaultFileLocation;
        private int _recentFilesCount;
        private bool _showStartScreen;
        private Dictionary<string, object> _moduleSettings;

        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public bool AutoSave
        {
            get => _autoSave;
            set => SetProperty(ref _autoSave, value);
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set => SetProperty(ref _autoSaveInterval, value);
        }

        public string DefaultFileLocation
        {
            get => _defaultFileLocation;
            set => SetProperty(ref _defaultFileLocation, value);
        }

        public int RecentFilesCount
        {
            get => _recentFilesCount;
            set => SetProperty(ref _recentFilesCount, value);
        }

        public bool ShowStartScreen
        {
            get => _showStartScreen;
            set => SetProperty(ref _showStartScreen, value);
        }

        public Dictionary<string, object> ModuleSettings
        {
            get => _moduleSettings ??= new Dictionary<string, object>();
            set => SetProperty(ref _moduleSettings, value);
        }

        public ApplicationSettings()
        {
            _theme = "Light";
            _language = "en-US";
            _autoSave = true;
            _autoSaveInterval = 5;
            _recentFilesCount = 10;
            _showStartScreen = true;
            _moduleSettings = new Dictionary<string, object>();
        }
    }
}
