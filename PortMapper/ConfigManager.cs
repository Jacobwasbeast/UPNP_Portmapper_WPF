using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using Mono.Nat;
using Newtonsoft.Json;
using Portmapper;

namespace PortMapper
{
    public class Config
    {
        public ObservableCollection<PortMapping> PortMappings { get; set; } = new ObservableCollection<PortMapping>();
    }

    public static class ConfigManager
    {
        private static readonly string ConfigFilePath = "config.json";

        public static void SaveConfig(Config config)
        {
            var json = JsonConvert.SerializeObject(config,  Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
        }

        public static Config LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                Config config = JsonConvert.DeserializeObject<Config>(json);
                return config;
            }

            return new Config(); // Return default config if no file found
        }
    }
}