using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAss
{
    internal class CheckDirCommand
    {
        private readonly ConfigManager _configManager;

        public CheckDirCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>
        /// Check if a directory exists and return "exists" or "missing" to console or output file.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns>True if check completed, false if path is invalid</returns>
        public bool Execute(string dirPath)
        {
            var config = _configManager.GetConfig();
            if (!config.Features.CheckDir)
            {
                OutputManager.WriteLine("CheckDir command is disabled in configuration.");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(dirPath))
                {
                    OutputManager.WriteLine("Error: No directory path specified for check-dir command.");
                    return false;
                }
                var fullPath = Path.GetFullPath(dirPath);

                if (!Directory.Exists(fullPath))
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
