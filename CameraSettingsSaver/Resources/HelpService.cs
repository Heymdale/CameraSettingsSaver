using System;
using System.Collections.Generic;

namespace CameraSettingsSaver.Resources
{
    public class Help
    {
        public static string GetLineSeparator()
        {
            return "=".PadRight(50, '=');
        }

        public string GetUserGuide(Localization localization)
        {
            var guideParts = new List<string>
            {
                localization.GetString("UserGuide.Title"),
                "",
                localization.GetString("UserGuide.Section1Title"),
                ""
            };

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section1Save"));
            guideParts.Add("");

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section1Apply"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.Section2Title"));
            guideParts.Add("");

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section2Content"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.Section3Title"));
            guideParts.Add("");

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section3Content"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.Section4Title"));
            guideParts.Add("");

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section4Content"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.Section5Title"));
            guideParts.Add("");

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section5Content"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.Section6Title"));
            guideParts.Add("");

            AddMultilineString(guideParts, localization.GetString("UserGuide.Section6Content"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.QuickStartTitle"));
            guideParts.Add("");

            guideParts.Add(localization.GetString("UserGuide.QuickStart1"));
            guideParts.Add(localization.GetString("UserGuide.QuickStart2"));
            guideParts.Add(localization.GetString("UserGuide.QuickStart3"));
            guideParts.Add(localization.GetString("UserGuide.QuickStart4"));
            guideParts.Add(localization.GetString("UserGuide.QuickStart5"));
            guideParts.Add(localization.GetString("UserGuide.QuickStart6"));
            guideParts.Add("");

            guideParts.Add(GetLineSeparator());
            guideParts.Add(localization.GetString("UserGuide.ConsoleHelpTitle"));
            guideParts.Add(GetLineSeparator());
            guideParts.Add("");

            return string.Join(Environment.NewLine, guideParts) + GetConsoleHelp(localization);
        }

        public string GetServiceHelp(Localization localization)
        {
            var helpParts = new List<string>
            {
                GetLineSeparator(),
                $"   {localization.GetString("ServiceHelp.Title")}",
                GetLineSeparator(),
                "",
                localization.GetString("ServiceHelp.Description"),
                "",
                GetLineSeparator(),
                localization.GetString("ServiceHelp.Usage"),
                GetLineSeparator(),
                "",
                localization.GetString("ServiceHelp.UsageText"),
                "",
                localization.GetString("ServiceHelp.Options"),
                localization.GetString("ServiceHelp.OptionProfile"),
                localization.GetString("ServiceHelp.OptionLang"),
                localization.GetString("ServiceHelp.OptionInterval"),
                localization.GetString("ServiceHelp.OptionLog"),
                localization.GetString("ServiceHelp.OptionHelp"),
                "",
                localization.GetString("ServiceHelp.InstallNote"),
                "",
                localization.GetString("ServiceHelp.ScCommandTitle"),
                "",
                localization.GetString("ServiceHelp.ScInstall"),
                localization.GetString("ServiceHelp.ScStart"),
                localization.GetString("ServiceHelp.ScStop"),
                localization.GetString("ServiceHelp.ScDelete"),
                "",
                localization.GetString("ServiceHelp.EasierWay"),
                "",
                GetLineSeparator()
            };

            return string.Join(Environment.NewLine, helpParts);
        }

        private void AddMultilineString(List<string> parts, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            string[] lines = text.Split(new[] { "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                parts.Add(line.TrimEnd('\r'));
            }
        }

        public string GetConsoleHelp(Localization localization)
        {
            var helpParts = new List<string>
            {
                "",
                GetLineSeparator(),
                $"   {localization.GetString("Console.Header")}",
                GetLineSeparator(),
                "",
                localization.GetString("UserGuide.ConsoleHelpTitle"),
                GetLineSeparator(),
                "",
                localization.GetString("UserGuide.ConsoleHelpUsage"),
                localization.GetString("UserGuide.ConsoleHelpUsageText"),
                "",
                localization.GetString("UserGuide.ConsoleHelpOptions"),
                localization.GetString("UserGuide.ConsoleHelpOptionC"),
                localization.GetString("UserGuide.ConsoleHelpOptionNI"),
                localization.GetString("UserGuide.ConsoleHelpOptionP"),
                localization.GetString("UserGuide.ConsoleHelpOptionL"),
                localization.GetString("UserGuide.ConsoleHelpOptionLang"),
                localization.GetString("UserGuide.ConsoleHelpOptionHelp"),
                "",
                localization.GetString("UserGuide.ConsoleHelpExamples"),
                localization.GetString("UserGuide.ConsoleHelpExampleGui"),
                localization.GetString("UserGuide.ConsoleHelpExampleGui1"),
                localization.GetString("UserGuide.ConsoleHelpExampleGui2"),
                "",
                localization.GetString("UserGuide.ConsoleHelpExampleConsole"),
                localization.GetString("UserGuide.ConsoleHelpExampleConsole1"),
                localization.GetString("UserGuide.ConsoleHelpExampleConsole2"),
                localization.GetString("UserGuide.ConsoleHelpExampleConsole3"),
                localization.GetString("UserGuide.ConsoleHelpExampleLang"),
                "",
                localization.GetString("UserGuide.ConsoleHelpExampleBackground"),
                localization.GetString("UserGuide.ConsoleHelpExampleBackground1"),
                localization.GetString("UserGuide.ConsoleHelpExampleBackground2"),
                "",
                localization.GetString("UserGuide.ConsoleHelpFileFormat"),
                localization.GetString("UserGuide.ConsoleHelpFileFormatText"),
                localization.GetString("UserGuide.ConsoleHelpDefaultFile"),
                "",
                localization.GetString("UserGuide.ConsoleHelpExitCodes"),
                localization.GetString("UserGuide.ConsoleHelpExitCode0"),
                localization.GetString("UserGuide.ConsoleHelpExitCode1"),
                localization.GetString("UserGuide.ConsoleHelpExitCode2"),
                localization.GetString("UserGuide.ConsoleHelpExitCode3"),
                "",
                GetLineSeparator()
            };

            return string.Join(Environment.NewLine, helpParts);
        }
    }
}