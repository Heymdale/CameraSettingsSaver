using System.Reflection;
using WebCamSettings.Core;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Subscribe to search for DLLs in the bin folder
        AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
        {
            var name = new AssemblyName(eventArgs.Name).Name + ".dll";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", name);
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };

        // Run the logic
        new ApplicationRunner().Run(args);
    }
}