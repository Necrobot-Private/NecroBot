using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Common
{
    public class UITranslation
    {
        #region Main screen
        [Description("Map & Journey")]
        public string MapTabTitle { get; set; }

        [Description("Sniper")]
        public string SniperTabTitle { get; set; }
        [Description("Bot switching account... ")]
        public string AccountSwitching { get; set; }

        [Description("Eggs")]
        public string EggTabHeader { get; set; }

        [Description("Console")]
        public string ConsoleTabTitle { get; set; }

        [Description("Pokemon [{0}/{1}]")]
        public string PokemonTabTitle { get; set; }

        [Description("Show Console")]
        public string ShowConsole { get; set; }

        [Description("Hide Console")]
        public string HideConsole { get; set; }

        [Description("Setting")]
        public string MenuSetting { get; set; }

        [Description("Theme")]
        public string Theme { get; set; }

        #endregion

        #region Popup
        [Description("Latitude")]
        public string Latitude { get; set; }

        [Description("Longitude")]
        public string Longitude { get; set; }


        [Description("Distance")]
        public string Distance { get; set; }


        [Description("Close")]
        public string Close { get; set; }


        [Description("WalkHere")]
        public string WalkToHere { get; set; }

        [Description("CP")]
        public string GymDefenderCP { get; set; }

        [Description("Gym Point")]
        public string GymPoints { get; set; }


        #endregion
        private Dictionary<string, string> translations = new Dictionary<string, string>();
        private string languageCode = "en";
        private string translationFile = @"Config\Translations\ui.{0}.json";
        public UITranslation(string language = "en")
        {
            languageCode = language;

            this.translationFile = string.Format(translationFile, language);

            Load();
        }

        public string GetTranslation(string key)
        {
            var prop = GetType().GetProperty(key);
            if(prop != null)
            {
                return prop.GetValue(this).ToString();
            }
            return $"{key} missing";
        }

        public void Save()
        {
            var type = this.GetType();
            foreach (var item in type.GetProperties())
            {
                if (translations.ContainsKey(item.Name)) continue;
                translations.Add(item.Name, item.GetValue(this).ToString());

            }

            File.WriteAllText(this.translationFile, JsonConvert.SerializeObject(translations, Formatting.Indented));
        }
        public void Load()
        {
            var type = this.GetType();

            var props = this.GetType().GetProperties();

            foreach (var pi in props)
            {
                var description = pi.GetCustomAttribute<DescriptionAttribute>();
                if (description != null)
                {
                    pi.SetValue(this, description.Description);
                }
                else
                pi.SetValue(this, pi.Name);
            }

            if (File.Exists(translationFile))
            {
                translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(translationFile));

                foreach (var item in translations.Keys)
                {
                    {
                        var curent = type.GetProperty(item);

                        if (curent != null)
                        {
                            curent.SetValue(this, translations[item]);
                        }

                    }
                }
            }
            Save();

        }

    }
}
