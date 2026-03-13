using CameraSettingsSaver.Resources;
using CameraSettingsWindowsService.Models;
using CameraSettingsWindowsService.Services;
using CameraSettingsWindowsService.Core;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

var localization = new Localization();

// DETERMINE THE START-UP MODE
// 1. Check if it is running in interactive mode (from the command line)
bool isInteractive = Environment.UserInteractive;

// 2. Check the forced console mode flag
bool forceConsoleMode = args.Contains("--console") || args.Contains("/console");

// 3. Service mode: not interactive and not forced console
bool isServiceMode = !isInteractive && !forceConsoleMode;

// Parse arguments taking into account the mode
var parser = new ArgumentParser(localization);
var settings = parser.Parse(args, isServiceMode);

localization.SetLanguage(settings.Language);

// If help is requested, show and exit
if (settings.IsShowHelp)
{
    parser.ShowHelp(settings);
    return;
}

// We display warnings and errors only in console mode
if (settings.IsConsoleMode)
{
    parser.ShowWarningsAndErrors();
}

// If there are critical errors in console mode, we terminate the work
if (parser.Errors.Any() && settings.IsConsoleMode)
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}

// Register services
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(localization);
builder.Services.AddSingleton(sp =>
{
    var logger = new BasicLogger(settings.LogFile ?? "");

    if (settings.IsConsoleMode)
    {
        logger.EnableConsoleOutput();
    }

    return logger;
});

// Add Windows Service support ONLY if it is a service mode
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && settings.IsServiceMode)
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "CameraSettingsService";
    });
}

builder.Services.AddHostedService<CameraSettingsService>();

var host = builder.Build();

// Run in the appropriate mode
if (settings.IsConsoleMode)
{
    Console.WriteLine(localization.GetString("Service.Starting"));
    Console.WriteLine(string.Format(localization.GetString("Service.ProfilePath"), settings.ProfilePath));
    Console.WriteLine(string.Format(localization.GetString("Service.Interval"), settings.IntervalSeconds));
    Console.WriteLine(string.Format(localization.GetString("Service.LogFile"),
        settings.LogFile ?? localization.GetString("Console.NoLogging")));
    Console.WriteLine($"Mode: Console (Interactive: {isInteractive})");
    Console.WriteLine(localization.GetString("Common.Information"));
    Console.WriteLine("Press Ctrl+C to stop the service...");

    await host.RunAsync();
}
else
{
    // Run as a Windows service (without console)
    host.Run();
}