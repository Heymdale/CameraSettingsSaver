using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Utils;
using System.Runtime.InteropServices;

namespace CameraSettingsSaver.Core
{
    public class ConsoleManager
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private const int ATTACH_PARENT_PROCESS = -1;

        public bool Initialize()
        {
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                if (GetConsoleWindow() == IntPtr.Zero)
                {
                    return AllocateNewConsole();
                }
            }
            return SetupConsoleOutput();
        }

        private bool AllocateNewConsole()
        {
            try
            {
                if (!AllocConsole()) return false;

                Console.Title = "Camera Settings Saver - Console Mode";
                return SetupConsoleOutput();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool SetupConsoleOutput()
        {
            try
            {
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void Cleanup()
        {
            FreeConsole();
        }

        public void ShowHeader(StartupMode startupMode, Localization localization, BasicLogger logger)
        {
            if (logger == null || localization == null) return;

            // BasicLogger doesn't have LogMessageNoTimestamp, so we use simple logging
            // but without a timestamp, creating a message and logging it directly

            string header = localization.lineSeparator;
            logger.Log(header);

            string title = $"   {localization.GetString("Console.Header")}";
            logger.Log(title);

            logger.Log(localization.lineSeparator);

            string configFileMessage = localization.GetString("Console.ConfigFile", startupMode.Profile);
            logger.Log(configFileMessage);

            string logFileMessage = localization.GetString("Console.LogFile",
                startupMode.LogFile ?? localization.GetString("Console.NoLogging"));
            logger.Log(logFileMessage);

            string timeMessage = localization.GetString("Console.Time",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            logger.Log(timeMessage);

            string languageMessage = localization.GetString("Console.CurrentLanguage",
                localization.CurrentLanguage);
            logger.Log(languageMessage);

            logger.Log(localization.lineSeparator);
            logger.Log("");
        }

        public void ShowFooter(BasicLogger logger)
        {
            logger?.Log("");
            logger?.Log("Press any key to exit...");
        }

        public void WaitForExit(bool isConsoleStart)
        {
            if (!isConsoleStart) return;
            Console.ReadKey();
        }
    }
}