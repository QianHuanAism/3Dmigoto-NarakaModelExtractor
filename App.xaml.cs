using NMC.Helpers;
using System.IO;
using System.Windows;

namespace NMC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            string logDirectory = Path.Combine(PathHelper.CurrentDirectory, "Logs");
            string logFilePath = Path.Combine(logDirectory, $"{Log.TimeStamp}.log");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            if (!File.Exists(logFilePath))
            {
                using (File.Create(logFilePath)) { }
            }

            File.WriteAllLines(logFilePath, Log.LogList);
        }
    }

}
