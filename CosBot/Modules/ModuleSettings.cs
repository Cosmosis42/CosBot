using CosBot.Modules.Modules;
using Discord;
using Discord.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosBot.Modules
{
    class ModuleSettings
    {
        static int NUM_MODULES = 5; // ALWAYS update this when a new module is created

        private const string path = "./config/modules.json";
        private static ModuleSettings _instance = new ModuleSettings();

        public static void Load()
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} is missing.");
            _instance = JsonConvert.DeserializeObject<ModuleSettings>(File.ReadAllText(path));
        }
        public static void Save()
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(_instance, Formatting.Indented));
        }

        // Users
        public class ModulesSettings
        {
            [JsonProperty("name")]
            public string name;
            [JsonProperty("server")]
            public ulong[] servers;
        }
        [JsonProperty("modules")]
        private ModulesSettings[] _modules = new ModulesSettings[NUM_MODULES];
        public static ModulesSettings[] Module => _instance._modules;

        // Rules
        public class RulesSettings
        {
            [JsonProperty("server")]
            public ulong[] server;
            [JsonProperty("ruleMessage")]
            public string[] ruleMessage;
        }
        [JsonProperty("rules")]
        private RulesSettings _rules = new RulesSettings();
        public static RulesSettings Rules => _instance._rules;
    }
}
