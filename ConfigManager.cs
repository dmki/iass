using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace IAss
{
    public class ConfigManager
    {
        private static readonly string ConfigFileName = "iass.conf.json";
        private Config? _config;

        public Config GetConfig()
        {
            if (_config == null)
            {
                LoadConfig();
            }
            return _config!;
        }

        public void LoadConfig()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
            
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new Config();
            }
            else
            {
                // Create default config file
                _config = new Config();
                SaveConfig(_config);
            }
        }

        public void SaveConfig(Config config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
            File.WriteAllText(configPath, json);
            _config = config;
        }
    }
}