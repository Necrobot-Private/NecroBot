using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Model.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace PoGo.NecroBot.Logic.Common
{
    public class UITranslation
    {
        #region Main screen

        [Description("Accounts")]
        public string AccountSetting { get; set; }

        [Description("SNIPE")]
        public string SnipeText { get; set; }

        [Description("ALL BOT SNIPE")]
        public string SnipeAllBotText { get; set; }

        [Description("Help")]
        public string Help { get; set; }

        [Description("Show Console")]
        public string ShowConsole { get; set; }

        [Description("Reset Layout")]
        public string ResetLayout { get; set; }

        [Description("Switch")]
        public string Switch { get; set; }

        [Description("Hide Console")]
        public string HideConsole { get; set; }

        [Description("Enable Hub")]
        public string EnableHub { get; set; }

        [Description("Disable Hub")]
        public string DisableHub { get; set; }

        [Description("Settings")]
        public string MenuSetting { get; set; }

        [Description("Theme")]
        public string Theme { get; set; }

        [Description("Scheme")]
        public string Scheme { get; set; }

        [Description("Enter your Command")]
        public string InputCommand { get; set; }

        #endregion
        #region Map

        [Description("Zoom In")]
        public string ZoomIn { get; set; }

        [Description("Zoom Out")]
        public string ZoomOut { get; set; }

        [Description("Clear Map")]
        public string ClearMap { get; set; }

        [Description("Walk Here")]
        public string WalkHere { get; set; }

        #endregion
        #region snipe screen

        [Description("100% IV")]
        public string TabSnipeIV100 { get; set; }

        [Description("Rare Pokemon")]
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
        [Description("TRANSFER CONFIRM?")]
        public string TransferConfirmCaption { get; set; }

        [Description("Types")]
        public string Types { get; set; }

        [Description("TRANSFER")]
        public string TransferConfirmButton { get; set; }

        [Description("Do you want to transfer {0} IV:{1:0.00}% Lvl:{2}")]
        public string TransferConfirmText { get; set; }

        [Description("Shiny")]
        public string Shiny { get; set; }

        [Description("Form")]
        public string Form { get; set; }

        [Description("Costume")]
        public string Costume { get; set; }

        [Description("Sex")]
        public string Sex { get; set; }

        [Description("Evolve Filter Setting")]
        public string MenuTransferFilterText { get; set; }

        [Description("Evolve Filter Setting")]
        public string MenuEvolveFilterText { get; set; }

        [Description("Snipe Filter Setting")]
        public string MenuSnipeFilterText { get; set; }

        [Description("Snipe Upgrade Setting")]
        public string MenuUpgradeFilterText { get; set; }

        [Description("This pokemon can be evolve to below pokemon , please select the branch you want to evolve to")]
        public string EvolveConfirm { get; set; }

        [Description("Evolve Pokemon")]
        public string EvolvePopupCaption { get; set; }

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
        public string SearchSelectAllButton { get; set; }

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

        [Description("Experience")]
        public string ExperienceInfo { get; set; }

        [Description("Caught at")]
        public string CaughtTime { get; set; }

        [Description("Location")]
        public string CaughtLocation { get; set; }

        [Description("Set Buddy")]
        public string SetBuddy { get; set; }

        [Description("Actions")]
        public string Actions { get; set; }

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

        [Description("Walk Here")]
        public string WalkToHere { get; set; }

        [Description("CP")]
        public string GymDefenderCP { get; set; }

        [Description("Gym Points")]
        public string GymPoints { get; set; }

        [Description("Pokestops: {0}")]
        public string PokestopLimit { get; set; }

        [Description("Pokemons: {0}")]
        public string CatchLimit { get; set; }

        [Description("Speed: {0:0.00} km/h")]
        public string WalkSpeed { get; set; }

        [Description("Transfered: {0}")]
        public string PokemonTransfered { get; set; }

        [Description("HIDE")]
        public string Hide { get; set; }

        [Description("SHOW")]
        public string Show { get; set; }

        [Description("Transfer filter - {0}")]
        public string TransferFilterFormTitle { get; set; }
        #endregion

        private Dictionary<string, string> translations = new Dictionary<string, string>();
        private string languageCode = "en";
        private string translationFile = @"Config\Translations\ui.{0}.json";
        public UITranslation(string language = "en")
        {
            languageCode = language;

            translationFile = string.Format(translationFile, language);

            Load();
        }

        public string GetTranslation(string key)
        {
            var prop = GetType().GetProperty(key);
            if (prop != null)
            {
                return prop.GetValue(this).ToString();
            }
            if (translations.ContainsKey(key))
            {
                return translations[key];
            }
            return $"{key} missing";
        }

        public void Save()
        {
            var type = GetType();
            foreach (var item in type.GetProperties())
            {
                if (translations.ContainsKey(item.Name)) continue;
                translations.Add(item.Name, item.GetValue(this).ToString());
            }

            File.WriteAllText(translationFile, JsonConvert.SerializeObject(translations, Formatting.Indented));
        }
        public void Load()
        {
            var type = GetType();

            var props = GetType().GetProperties();

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
            //append translation for setting
            Type setting = typeof(GlobalSettings);

            foreach (var item in setting.GetFields())
            {
                var configAttibute = item.GetCustomAttribute<NecroBotConfigAttribute>();
                if (configAttibute != null)
                {
                    var fileName = !string.IsNullOrEmpty(configAttibute.SheetName) ? configAttibute.SheetName : item.Name;

                    string key = $"Setting.{item.Name}";
                    var configType = item.FieldType;

                    if (!translations.ContainsKey(key))
                    {
                        translations.Add(key, !string.IsNullOrEmpty(configAttibute.Key) ? configAttibute.Key : fileName);
                    }
                    var keyDesc = $"{key}Desc";

                    if (!translations.ContainsKey(keyDesc))
                    {
                        translations.Add(keyDesc, !string.IsNullOrEmpty(configAttibute.Description) ? configAttibute.Description : $"{key} description");
                    };
                    if (item.FieldType.IsGenericType && (item.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    {
                        Type keyType = item.FieldType.GetGenericArguments()[0];
                        Type valueType = item.FieldType.GetGenericArguments()[1];
                        AddResourceForType(key, valueType);
                    }

                    if (item.FieldType.IsGenericType && (item.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                    {
                        Type keyType = item.FieldType.GetGenericArguments()[0];
                        AddResourceForType(key, keyType);
                    }


                    AddResourceForType(key, configType);
                }

                Save();

            }

        }

        private void AddResourceForType(string key, Type configType)
        {
            foreach (var configItem in configType.GetProperties())
            {
                var propAttibute = configItem.GetCustomAttribute<NecroBotConfigAttribute>();
                if (propAttibute != null)
                {
                    string fieldValue = !string.IsNullOrEmpty(propAttibute.Key) ? propAttibute.Key : configItem.Name;

                    var subKey = $"{key}.{configItem.Name}";
                    var descKey = subKey + "Desc";

                    if (!translations.ContainsKey(subKey))
                    {
                        translations.Add(subKey, string.IsNullOrEmpty(propAttibute.Key) ? fieldValue : propAttibute.Key);
                    }

                    if (!translations.ContainsKey(descKey))
                    {
                        translations.Add(descKey, !string.IsNullOrEmpty(propAttibute.Description) ? propAttibute.Description : $"{subKey} description");
                    }
                }
            }
        }
    }
}