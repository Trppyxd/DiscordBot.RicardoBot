using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DiscordBot_BlueBot;
using Newtonsoft.Json;

namespace DiscordBot.BlueBot
{
    class DataStorage
    {
        public static Dictionary<string, string> pairs = new Dictionary<string, string>();
        public static void AddPairToStorage(string key, string value)
        {
            pairs.Add(key, value);
            SaveData();
        }

        public static int GetPairsCount()
        {
            return pairs.Count();
        }

        static DataStorage()
        {
            // Load data
            if (!Utilities.ValidateFileExistance("DataStorage.json")) return;
            string json = File.ReadAllText("DataStorage.json");
            pairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static void SaveData()
        {
            // Save data
            string json = JsonConvert.SerializeObject(pairs, Formatting.Indented);
            File.WriteAllText("DataStorage.json", json);
        }
    }
}
