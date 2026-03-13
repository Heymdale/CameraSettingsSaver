using Microsoft.Extensions.Hosting;
using System.Timers;
using Timer = System.Timers.Timer;

public class Worker : BackgroundService
{
    private Timer _timer;
    private readonly string _logFilePath;
    private readonly object _lockObject = new object();

    public Worker()
    {
        // Path to the log file next to the executable file
        string appFolder = AppDomain.CurrentDomain.BaseDirectory;
        _logFilePath = Path.Combine(appFolder, "log.txt");

        Console.WriteLine($"Log file: {_logFilePath}");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a timer for 20 seconds (20000 milliseconds)
        _timer = new Timer(20000); // 20 seconds
        _timer.Elapsed += WriteToLog;
        _timer.AutoReset = true; // Repeat automatically
        _timer.Enabled = true;

        // Write the first line immediately upon startup
        WriteToLog(null, null);

        Console.WriteLine("Service started. Writing to log every 20 seconds...");

        return Task.CompletedTask;
    }

    private void WriteToLog(object sender, ElapsedEventArgs e)
    {
        // Lock in case of concurrent access
        lock (_lockObject)
        {
            try
            {
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log entry";

                // Write to file (append, do not overwrite)
                File.AppendAllText(_logFilePath, message + Environment.NewLine);

                // We also write to the console for debugging
                Console.WriteLine($"Written: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log: {ex.Message}");
            }
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Stop();
        _timer?.Dispose();

        string stopMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Service stopped";
        File.AppendAllText(_logFilePath, stopMessage + Environment.NewLine);
        Console.WriteLine("Service stopped");

        return Task.CompletedTask;
    }
}