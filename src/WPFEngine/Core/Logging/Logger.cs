using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace WPFEngine.Core.Logging
{
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

    public interface ILogger
    {
        void Log(LogLevel level, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
        void LogTrace(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
        void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
        void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
        void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
        void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
        void LogCritical(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
    }

    public class ConsoleLogger : ILogger
    {
        private readonly object _lock = new object();
        private LogLevel _minLevel;

        public LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        public ConsoleLogger(LogLevel minLevel = LogLevel.Info)
        {
            _minLevel = minLevel;
        }

        public void Log(LogLevel level, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            if (level < _minLevel) return;

            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelStr = level.ToString().ToUpper().PadRight(8);
                var fileName = Path.GetFileName(file);
                var formattedMessage = $"[{timestamp}] [{levelStr}] [{fileName}:{line}] {caller}: {message}";

                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = GetColor(level);
                Console.WriteLine(formattedMessage);
                Console.ForegroundColor = originalColor;
            }
        }

        public void LogTrace(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Trace, message, caller, file, line);

        public void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Debug, message, caller, file, line);

        public void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Info, message, caller, file, line);

        public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Warning, message, caller, file, line);

        public void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Error, message, caller, file, line);

        public void LogCritical(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Critical, message, caller, file, line);

        private static ConsoleColor GetColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
        }
    }

    public class FileLogger : ILogger
    {
        private readonly object _lock = new object();
        private readonly string _filePath;
        private readonly bool _append;
        private LogLevel _minLevel;
        private StreamWriter? _writer;

        public LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        public FileLogger(string filePath, bool append = false, LogLevel minLevel = LogLevel.Debug)
        {
            _filePath = filePath;
            _append = append;
            _minLevel = minLevel;
            Initialize();
        }

        private void Initialize()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(_filePath, _append)
            {
                AutoFlush = true
            };
        }

        public void Log(LogLevel level, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            if (level < _minLevel || _writer == null) return;

            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelStr = level.ToString().ToUpper().PadRight(8);
                var fileName = Path.GetFileName(file);
                var formattedMessage = $"[{timestamp}] [{levelStr}] [{fileName}:{line}] {caller}: {message}";
                _writer.WriteLine(formattedMessage);
            }
        }

        public void LogTrace(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Trace, message, caller, file, line);

        public void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Debug, message, caller, file, line);

        public void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Info, message, caller, file, line);

        public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Warning, message, caller, file, line);

        public void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Error, message, caller, file, line);

        public void LogCritical(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Critical, message, caller, file, line);

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> _loggers = new List<ILogger>();

        public void AddLogger(ILogger logger)
        {
            _loggers.Add(logger);
        }

        public void RemoveLogger(ILogger logger)
        {
            _loggers.Remove(logger);
        }

        public void ClearLoggers()
        {
            _loggers.Clear();
        }

        public void Log(LogLevel level, string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(level, message, caller, file, line);
            }
        }

        public void LogTrace(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Trace, message, caller, file, line);

        public void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Debug, message, caller, file, line);

        public void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Info, message, caller, file, line);

        public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Warning, message, caller, file, line);

        public void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Error, message, caller, file, line);

        public void LogCritical(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Critical, message, caller, file, line);
    }
}
