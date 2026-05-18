using System;
using System.IO;
using System.Threading.Tasks;
using OfficeSuite.Core.Models;

namespace OfficeSuite.Core.Services
{
    public interface IFileService
    {
        Task<Stream> OpenAsync(string filePath);
        Task SaveAsync(string filePath, Stream stream);
        Task<bool> ExistsAsync(string filePath);
        Task DeleteAsync(string filePath);
        Task<FileInfo> GetFileInfoAsync(string filePath);
        string GetDefaultDirectory();
        string[] GetRecentFiles();
        void AddToRecentFiles(string filePath);
    }

    public class FileService : IFileService
    {
        private readonly string[] _recentFiles = new string[20];
        private int _recentIndex = 0;
        private readonly string _defaultDirectory;

        public FileService()
        {
            _defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public async Task<Stream> OpenAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return await Task.FromResult(File.OpenRead(filePath));
        }

        public async Task SaveAsync(string filePath, Stream stream)
        {
            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            AddToRecentFiles(filePath);
        }

        public Task<bool> ExistsAsync(string filePath)
        {
            return Task.FromResult(File.Exists(filePath));
        }

        public Task DeleteAsync(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.CompletedTask;
        }

        public Task<FileInfo> GetFileInfoAsync(string filePath)
        {
            if (File.Exists(filePath))
                return Task.FromResult(new FileInfo(filePath));
            return Task.FromResult<FileInfo>(null);
        }

        public string GetDefaultDirectory()
        {
            return _defaultDirectory;
        }

        public string[] GetRecentFiles()
        {
            var files = new System.Collections.Generic.List<string>();
            for (int i = 0; i < _recentFiles.Length; i++)
            {
                if (!string.IsNullOrEmpty(_recentFiles[i]))
                    files.Add(_recentFiles[i]);
            }
            return files.ToArray();
        }

        public void AddToRecentFiles(string filePath)
        {
            for (int i = 0; i < _recentFiles.Length; i++)
            {
                if (_recentFiles[i] == filePath)
                {
                    for (int j = i; j > 0; j--)
                        _recentFiles[j] = _recentFiles[j - 1];
                    _recentFiles[0] = filePath;
                    return;
                }
            }

            for (int i = _recentFiles.Length - 1; i > 0; i--)
                _recentFiles[i] = _recentFiles[i - 1];
            _recentFiles[0] = filePath;
        }
    }

    public interface IExportService
    {
        Task<byte[]> ExportAsync(object document, ExportFormat format);
        bool CanExport(ExportFormat format);
    }

    public enum ExportFormat
    {
        Native,
        PDF,
        HTML,
        RTF,
        Text,
        CSV,
        Image
    }

    public class ExportService : IExportService
    {
        public Task<byte[]> ExportAsync(object document, ExportFormat format)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public bool CanExport(ExportFormat format)
        {
            return true;
        }
    }

    public interface IDialogService
    {
        string ShowOpenFileDialog(string filter, string title = null);
        string ShowSaveFileDialog(string filter, string defaultFileName = null, string title = null);
        string ShowFolderBrowserDialog(string description = null);
        bool ShowConfirmationDialog(string message, string title = "Confirm");
        void ShowMessageDialog(string message, string title = "Information");
        void ShowErrorDialog(string message, string title = "Error");
    }

    public class DialogService : IDialogService
    {
        public string ShowOpenFileDialog(string filter, string title = null)
        {
            return null;
        }

        public string ShowSaveFileDialog(string filter, string defaultFileName = null, string title = null)
        {
            return null;
        }

        public string ShowFolderBrowserDialog(string description = null)
        {
            return null;
        }

        public bool ShowConfirmationDialog(string message, string title = "Confirm")
        {
            return false;
        }

        public void ShowMessageDialog(string message, string title = "Information")
        {
        }

        public void ShowErrorDialog(string message, string title = "Error")
        {
        }
    }

    public interface ILoggingService
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception = null);
        void LogDebug(string message);
    }

    public class LoggingService : ILoggingService
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now}: {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARNING] {DateTime.Now}: {message}");
        }

        public void LogError(string message, Exception exception = null)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now}: {message}");
            if (exception != null)
                Console.WriteLine(exception);
        }

        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {DateTime.Now}: {message}");
        }
    }
}
