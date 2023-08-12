using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supdate
{
    public class ProcessStarter
    {
        public static void StartProcess(string path, string arguments = "")
        {
            // Create a new ProcessStartInfo to configure how the process will start
            ProcessStartInfo startInfo = new ProcessStartInfo(path, arguments)
            {
                // The path to the executable
                UseShellExecute = true,     // Use the shell to execute (allows things like file associations)
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false      // Whether to create a separate window for the process
            };

            try
            {
                // Start the process
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                ConsoleLog.Fatality("An error occurred starting a program: " + ex.Message);

            }
        }
    }
}
