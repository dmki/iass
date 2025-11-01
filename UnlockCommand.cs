using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using System.ServiceProcess;

namespace IAss
{
    public class UnlockCommand
    {
        private readonly ConfigManager _configManager;

        public UnlockCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool Execute(string fileName)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.Unlock)
            {
                OutputManager.WriteLine("Unlock command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    OutputManager.WriteLine("Error: No file name specified for unlock command.");
                    return false;
                }

                var filePath = Path.GetFullPath(fileName);
                
                if (!File.Exists(filePath))
                {
                    OutputManager.WriteLine($"File does not exist: {filePath}");
                    return false;
                }

                return UnlockFile(filePath);
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error in unlock command: {ex.Message}");
                return false;
            }
        }

        private bool UnlockFile(string filePath)
        {
            bool success = true;
            
            // First, try to remove NTFS attributes that might block the file
            try
            {
                var fileAttributes = File.GetAttributes(filePath);
                if (fileAttributes.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(filePath, fileAttributes & ~FileAttributes.ReadOnly);
                    OutputManager.WriteLine($"Removed read-only attribute from {filePath}");
                }
                
                if (fileAttributes.HasFlag(FileAttributes.System))
                {
                    File.SetAttributes(filePath, fileAttributes & ~FileAttributes.System);
                    OutputManager.WriteLine($"Removed system attribute from {filePath}");
                }
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error removing file attributes: {ex.Message}");
                success = false;
            }

            // Try to change NTFS permissions to Everyone full control
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileSecurity = fileInfo.GetAccessControl();
                var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                var everyoneAccount = new NTAccount("Everyone");
                
                // Add full control for Everyone
                fileSecurity.AddAccessRule(new FileSystemAccessRule(
                    everyoneAccount,
                    FileSystemRights.FullControl,
                    AccessControlType.Allow
                ));
                
                fileInfo.SetAccessControl(fileSecurity);
                OutputManager.WriteLine($"Set full control permissions for Everyone on {filePath}");
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error setting file permissions: {ex.Message}");
                success = false;
            }

            // Check if file is locked by another process and identify the process
            if (!CanWriteToFile(filePath))
            {
                OutputManager.WriteLine($"File is still locked. Trying to identify the locking process...");
                
                var lockingProcess = GetLockingProcess(filePath);
                if (lockingProcess != null)
                {
                    OutputManager.WriteLine($"File is locked by process: {lockingProcess.ProcessName} (PID: {lockingProcess.Id})");
                    
                    // If it's a service, try to restart the service
                    if (IsSystemService(lockingProcess.ProcessName))
                    {
                        OutputManager.WriteLine($"Detected system service. Attempting to restart service...");
                        if (RestartService(lockingProcess.ProcessName))
                        {
                            OutputManager.WriteLine($"Service {lockingProcess.ProcessName} restarted successfully.");
                        }
                        else
                        {
                            OutputManager.WriteLine($"Failed to restart service {lockingProcess.ProcessName}");
                        }
                    }
                    else
                    {
                        // Kill the process that is locking the file
                        OutputManager.WriteLine($"Terminating process {lockingProcess.ProcessName} (PID: {lockingProcess.Id})...");
                        try
                        {
                            lockingProcess.Kill();
                            lockingProcess.WaitForExit(5000); // Wait up to 5 seconds
                            
                            if (!lockingProcess.HasExited)
                            {
                                lockingProcess.Kill(true); // Force kill if still running
                            }
                            
                            OutputManager.WriteLine($"Process {lockingProcess.ProcessName} terminated.");
                        }
                        catch (Exception ex)
                        {
                            OutputManager.WriteLine($"Error terminating process: {ex.Message}");
                            success = false;
                        }
                    }
                }
                else
                {
                    OutputManager.WriteLine($"Could not identify the process locking the file.");
                }
            }
            
            // Final check if file can be written to
            if (CanWriteToFile(filePath))
            {
                OutputManager.WriteLine($"File unlocked successfully: {filePath}");
            }
            else
            {
                OutputManager.WriteLine($"Warning: File may still be locked: {filePath}");
                success = false;
            }
            
            return success;
        }

        private bool CanWriteToFile(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    fs.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Function to identify which process is locking a file
        private Process? GetLockingProcess(string filePath)
        {
            try
            {
                // Use handle.exe (part of Sysinternals tools) to find handles to the file
                string handlePath = "handle.exe";
                
                // Check if handle exists in current directory
                string localHandle = Path.Combine(AppContext.BaseDirectory, "handle.exe");
                if (File.Exists(localHandle))
                {
                    handlePath = localHandle;
                }
                
                // Try to find handle.exe in PATH
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "handle",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var whereProcess = Process.Start(processStartInfo);
                if (whereProcess != null)
                {
                    whereProcess.WaitForExit();
                    if (whereProcess.ExitCode == 0)
                    {
                        string handleLocation = whereProcess.StandardOutput.ReadToEnd().Trim();
                        if (!string.IsNullOrEmpty(handleLocation))
                        {
                            handlePath = handleLocation.Split('\n')[0].Trim(); // Get first result
                        }
                    }
                }
                
                // Try to run handle.exe to find the locking process
                var handleStartInfo = new ProcessStartInfo
                {
                    FileName = handlePath,
                    Arguments = $"\"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var handleProcess = Process.Start(handleStartInfo);
                if (handleProcess != null)
                {
                    string output = handleProcess.StandardOutput.ReadToEnd();
                    handleProcess.WaitForExit();
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        // Parse the output to find the PID
                        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (line.Contains(filePath, StringComparison.OrdinalIgnoreCase))
                            {
                                // Look for the PID (usually appears after the process name)
                                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var part in parts)
                                {
                                    if (int.TryParse(part.Replace("pid:", ""), out int pid))
                                    {
                                        try
                                        {
                                            return Process.GetProcessById(pid);
                                        }
                                        catch (ArgumentException)
                                        {
                                            // Process doesn't exist anymore
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Could not use handle.exe to find locking process: {ex.Message}");
            }
            
            // Alternative approach: try to find process using WMI (simplified)
            try
            {
                var allProcesses = Process.GetProcesses();
                foreach (var process in allProcesses)
                {
                    try
                    {
                        var modules = process.Modules;
                        foreach (ProcessModule module in modules)
                        {
                            if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                            {
                                return process;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore processes we don't have access to
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error checking processes for file lock: {ex.Message}");
            }
            
            return null;
        }

        private bool IsSystemService(string processName)
        {
            try
            {
                using (var serviceController = new ServiceController(processName))
                {
                    // Try to get service info - if successful, it's likely a service
                    var status = serviceController.Status;
                    return true;
                }
            }
            catch
            {
                // If it fails, it's not a service
                return false;
            }
        }

        private bool RestartService(string serviceName)
        {
            try
            {
                using (var serviceController = new ServiceController(serviceName))
                {
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        
                        return true;
                    }
                    else if (serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                OutputManager.WriteLine($"Error restarting service {serviceName}: {ex.Message}");
            }
            
            return false;
        }
    }
}