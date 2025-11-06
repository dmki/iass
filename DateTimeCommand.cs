using System;

namespace IAss
{
    internal class DateTimeCommand
    {
        private readonly ConfigManager _configManager;

        public DateTimeCommand(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        /// <summary>
        /// Returns the current date and time in ISO 8601 format (yyyy-MM-ddTHH:mm:ssZ).
        /// </summary>
        /// <returns>True if command executed successfully, false if disabled</returns>
        public bool Execute()
        {
            var config = _configManager.GetConfig();
            if (!config.Features.DateTime)
            {
                OutputManager.WriteLine("DateTime command is disabled in configuration.");
                return false;
            }

            try
            {
                var currentDateTime = System.DateTime.UtcNow;
                var formattedDateTime = currentDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                OutputManager.WriteLine(formattedDateTime);
                return true;
            }
            catch (Exception e)
            {
                OutputManager.WriteLine($"Error: {e.Message}");
                return false;
            }
        }
    }
}
