using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAss
{
    internal class CheckFileCommand
    {
        private readonly ConfigManager _configManager;

        public CheckFileCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>
        /// Check if a file exists and return "exists" or "missing" to console or output file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>True if check completed, false if path is invalid</returns>
        public bool Execute(string filePath)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.CheckFile)
            {
                OutputManager.WriteLine("CheckFile command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    OutputManager.WriteLine("Error: No file path specified for check-file command.");
                    return false;
                }
                var fullPath = Path.GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    OutputManager.WriteLine($"missing");
                    return true;
                }
                OutputManager.WriteLine($"exists");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
