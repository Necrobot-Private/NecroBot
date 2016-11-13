using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMasterConfig
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GlobalSettings.Load("");

            ExcelConfigHelper.MigrateFromObject(settings, "config\\config_migrated.xlsm");
            return;

            var newseting = ExcelConfigHelper.ReadExcel(settings, "config\\config_migrated.xlsm");
            Console.WriteLine(newseting.ConsoleConfig.TranslationLanguageCode);

            Console.WriteLine(newseting.WebsocketsConfig.WebSocketPort);

            Console.WriteLine(newseting.ConsoleConfig.DetailedCountsBeforeRecycling);
            Console.WriteLine(newseting.RecycleConfig.TotalAmountOfPokeballsToKeep);

            foreach (var item in newseting.ItemRecycleFilter)
            {
                Console.WriteLine($"{item.Key} => {item.Value}");
            }
            foreach (var item in newseting.PokemonToSnipe.Pokemon)
            {
                Console.WriteLine($"Pokemon to snipe ... {item}");
            }
            foreach (var item in newseting.HumanWalkSnipeFilters)
            {
                Console.WriteLine($"Pokemon to snipe ... {item.Key} => {item.Value.MaxDistance}");
            }

            foreach (var item in newseting.PokemonsTransferFilter)
            {
                Console.WriteLine($"Pokemon to snipe ... {item.Key} => {item.Value.KeepMinCp}");
            }
        }
    }
}
