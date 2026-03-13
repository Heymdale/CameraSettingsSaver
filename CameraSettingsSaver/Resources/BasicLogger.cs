using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CameraSettingsSaver.Resources
{
    public class BasicLogger
    {
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private readonly string? _logFilePath;
        private bool _consoleOutput = false;

        // Event for notification of new messages
        public event Action<string> OnLogMessage;

        public BasicLogger(string? logFileName)
        {
            if (logFileName != null)
            {
                // Logs will be one level higher than this dll
                string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string rootPath = Path.GetFullPath(Path.Combine(dllPath, ".."));
                _logFilePath = Path.Combine(rootPath, logFileName);
            }
        }

        public void EnableConsoleOutput()
        {
            _consoleOutput = true;
        }

        public virtual void Log(string message)
        {
            WriteLogEntry(message, "INFO");
        }

        public virtual void LogError(string message)
        {
            WriteLogEntry(message, "ERROR");
        }

        public virtual void LogWarning(string message)
        {
            WriteLogEntry(message, "WARNING");
        }

        private void WriteLogEntry(string message, string level)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] [{level}] {message}";

            // Save to buffer
            _logBuilder.AppendLine(logEntry);

            // Write to the console if enabled
            if (_consoleOutput)
            {
                WriteColoredToConsole(logEntry, level);
            }

            // Write to file
            WriteToFile(logEntry);

            // Notify subscribers
            OnLogMessage?.Invoke(logEntry);
        }

        private void WriteColoredToConsole(string message, string level)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            try
            {
                switch (level)
                {
                    case "ERROR":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "WARNING":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case "INFO":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                }

                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        private void WriteToFile(string logEntry)
        {
            try
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore errors writing to the file
            }
        }

        public string GetAllLogs()
        {
            return _logBuilder.ToString();
        }

        public void ClearLogs()
        {
            _logBuilder.Clear();
        }
    }
}