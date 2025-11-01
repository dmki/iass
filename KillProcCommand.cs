using System.Diagnostics;

namespace IAss
{
    public class KillProcCommand
    {
        private readonly ConfigManager _configManager;

        public KillProcCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool Execute(string identifier)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.KillProc)
            {
                OutputManager.WriteLine("KillProc command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    OutputManager.WriteLine("Error: No process identifier specified for killproc command.");
                    return false;
                }

                // Check if identifier is numeric (process ID)
                if (int.TryParse(identifier, out int processId))
                {
                    return KillProcessById(processId);
                }
                else
                {
                    return KillProcessByName(identifier);
                }
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error terminating process: {ex.Message}");
                return false;
            }
        }

        private bool KillProcessById(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                OutputManager.WriteLine($"Found process: {process.ProcessName} (PID: {process.Id})");
                
                // Try to kill using standard .NET method first
                process.Kill();
                process.WaitForExit(5000); // Wait up to 5 seconds for graceful exit
                
                if (!process.HasExited)
                {
                    process.Kill(true); // Force kill if still running
                }
                
                OutputManager.WriteLine($"Process {processId} terminated successfully.");
                return true;
            }
            catch (ArgumentException)
            {
                OutputManager.WriteLine($"Process with ID {processId} not found.");
                return false;
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error terminating process {processId}: {ex.Message}");
                
                // Fallback to pskill.exe if available
                return TryWithPskill(processId.ToString());
            }
        }

        private bool KillProcessByName(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                
                if (processes.Length == 0)
                {
                    OutputManager.WriteLine($"No processes found with name: {processName}");
                    return false;
                }

                bool success = true;
                foreach (var process in processes)
                {
                    try
                    {
                        OutputManager.WriteLine($"Terminating process: {process.ProcessName} (PID: {process.Id})");
                        process.Kill();
                        process.WaitForExit(5000); // Wait up to 5 seconds for graceful exit
                        
                        if (!process.HasExited)
                        {
                            process.Kill(true); // Force kill if still running
                        }
                        
                        OutputManager.WriteLine($"Process {process.ProcessName} (PID: {process.Id}) terminated successfully.");
                    }
                    catch (Exception ex)
                    {
                        OutputManager.WriteLine($"Error terminating process {process.ProcessName} (PID: {process.Id}): {ex.Message}");
                        success = false;
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error finding processes with name {processName}: {ex.Message}");
                
                // Fallback to pskill.exe if available
                return TryWithPskill(processName);
            }
        }

        private bool TryWithPskill(string identifier)
        {
            try
            {
                // Try to run pskill.exe from path or local directory
                string pskillPath = "pskill.exe";
                
                // Check if pskill exists in current directory
                string localPskill = Path.Combine(AppContext.BaseDirectory, "pskill.exe");
                if (File.Exists(localPskill))
                {
                    pskillPath = localPskill;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pskillPath,
                    Arguments = identifier,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    if (process.ExitCode == 0)
                    {
                        OutputManager.WriteLine($"pskill terminated process {identifier} successfully.");
                        if (!string.IsNullOrEmpty(output))
                            OutputManager.WriteLine(output);
                        return true;
                    }
                    else
                    {
                        OutputManager.WriteLine($"pskill failed with exit code {process.ExitCode}");
                        if (!string.IsNullOrEmpty(error))
                            OutputManager.WriteLine(error);
                        return false;
                    }
                }
                else
                {
                    OutputManager.WriteLine("Failed to start pskill.exe");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error running pskill.exe: {ex.Message}");
                return false;
            }
        }
    }
}