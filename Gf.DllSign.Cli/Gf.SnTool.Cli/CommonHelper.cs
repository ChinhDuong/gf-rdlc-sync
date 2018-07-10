using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Gf.SnTool.Cli
{
    public static class CommonHelper
    {
        public static int ExecuteCommand(string command, out string log, int timeout=0)
        {
            int ExitCode;
            ProcessStartInfo processInfo;

            string script = Path.Combine(Path.GetTempPath(), "myrun.bat");
            File.WriteAllText(script, command);
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput= true,
                Arguments = "/c " + script
            };
            Process process = new Process() { StartInfo = psi };
            process.Start();
            StreamReader reader = process.StandardOutput;
            log = reader.ReadToEnd();

            process.WaitForExit();
            ExitCode = process.ExitCode;
            process.Close();
            return ExitCode;
        }
        
    }
}
