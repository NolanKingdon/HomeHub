using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HomeHub.SystemUtils
{
    /// <summary>
    /// Wrapper class for Process that allows some standardization in terms of how we intend on querying the system.
    /// Intended to be used in a using statement, hence IDisposable. IDisposable will simply dispose the process.
    /// </summary>
    public class SystemProcess : IDisposable
    {
        private readonly ProcessStartInfo startInfo;
        private readonly Process process;

        public SystemProcess(ProcessStartInfo startInfo = null)
        {
            this.startInfo = startInfo ?? GenerateDefaultStartInfo();
            this.process = new Process();
        }

        /// <summary>
        /// Runs a command against a specific program (ie. )
        /// </summary>
        /// <param name="program">cmd.exe, qterminal, etc.</param>
        /// <param name="arguments">Actual arguments to run - ie df, ping xxx, etc.</param>
        /// <returns>Standard output as string</returns>
        public async Task<string> RunCommand(string program, string arguments)
        {
            string output;

            // Adding final options to the ProcessStartInfo object.
            startInfo.FileName = program;
            startInfo.Arguments = arguments;
            process.StartInfo = startInfo;

            // Starting the process.
            process.Start();

            output = await process.StandardOutput.ReadToEndAsync();

            process.WaitForExit();

            // Caller is responsible for formatting output.
            return output;
        }

        private static ProcessStartInfo GenerateDefaultStartInfo()
        {
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            return startInfo;
        }

        /// <summary>
        /// Implementing IDisposable.
        /// </summary>
        public void Dispose()
        {
            // Just closing out the process we open above.
            process.Dispose();
        }
    }
}
