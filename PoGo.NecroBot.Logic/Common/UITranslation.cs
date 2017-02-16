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
        [Description("Accounts")]
        public string AccountSetting { get; set; }
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

        [Description("Switch")]
        public string Switch { get; set; }


        [Description("Hide Console")]
        public string HideConsole { get; set; }

        [Description("Setting")]
        public string MenuSetting { get; set; }

        [Description("Theme")]
        public string Theme { get; set; }

        #endregion
        #region Map

        [Description("Zoom In")]
        public string ZoomIn { get; set; }


        [Description("ZoomOut")]
        public string ZoomOut { get; set; }

        [Description("Clear map")]
        public string ClearMap { get; set; }
        [Description("Walk Here")]
        public string WalkHere { get; set; }
        #endregion
        #region snipe screen
        [Description("100% IV")]
        public string TabSnipeIV100 { get; set; }

        [Description("Rare pokemon")]
        public string TabSnipeRarePokemon { get; set; }

        [Description("Others")]
        public string TabSnipeOtherPokemon { get; set; }

        [Description("Not in Dex")]
        public string TabSnipeNotInDexPokemon { get; set; }

        [Description("Auto Snipe List")]
        public string TabSnipeAutoListPokemon { get; set; }

        [Description("Add Manual Coordinate")]
        public string TabSnipeAddManualCoord { get; set; }
        [Description("Snipe??")]
        public string SnipeButton { get; set; }
        [Description("FreeInput")]
        public string FreeInput { get; set; }

        [Description("You can copy & paste any free text content which has pokemon name, latitude, longitude then bot will parse that content to get the snipe infomation.")]
        public string FreeInputExplain { get; set; }

        [Description("Add To Snipe")]
        public string AddToSnipeButtonText { get; set; }

        #endregion
        #region Pokemon Inventory
        [Description("Search & Filters")]
        public string FilterAndSearch { get; set; }

        [Description("Pokedex")]
        public string Pokedex { get; set; }
        [Description("Enter Pokemon Name")]
        public string SearchPokemonName { get; set; }
        [Description("Select Pokemon %IV")]
        public string SearchPokemonIV { get; set; }
        [Description("Select Pokemon Level")]
        public string SearchPokemonLevel { get; set; }

        [Description("Select Pokemon CP")]
        public string SearchPokemonCP { get; set; }

        [Description("Search & Select")]
        public string SearchSelectAllButton {get;set;}

        [Description("Search")]
        public string SearchButton { get; set; }

        [Description("Do you want to powerup this pokemon? Normal power up is do x time power up. Max power up is powerup to maximun level up to your candy, stardust and player level.")]
        public string PowerUpDescription { get; set; }
        [Description("Normal Power Up")]
        public string NormalPowerup { get; set; }

        [Description("Max Power Up")]
        public string MaxPowerup { get; set; }

        [Description("Export")]
        public string Export { get; set; }

        [Description("Verified")]
        public string Verified { get; set; }

        [Description("RemainTime")]
        public string RemainTime { get; set; }

        [Description("Name")]
        public string PokemonName { get; set; }


        [Description("HP")]
        public string HP { get; set; }



        [Description("Move1")]
        public string Move1 { get; set; }


        [Description("Move2")]
        public string Move2 { get; set; }


        [Description("IV")]
        public string IV { get; set; }

        [Description("CP")]
        public string CP { get; set; }

        [Description("Candy")]
        public string Candy { get; set; }

        [Description("Level")]
        public string Level { get; set; }

        [Description("Caught at")]
        public string CaughtTime { get; set; }

        [Description("Location")]
        public string CaughtLocation { get; set; }

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

        [Description("Pokestops: {0}")]
        public string PokestopLimit { get; set; }
        [Description("Pokemons: {0}")]
        public string CatchLimit { get; set; }
        [Description("Speed: {0:0.00} km/h")]
        public string WalkSpeed { get; set; }
        [Description("Transfered: {0}")]
        public string PokemonTransfered { get; set; }
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
            if (prop != null)
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
