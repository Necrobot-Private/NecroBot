using PoGo.NecroBot.Logic.Model.Settings;
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

            ExcelConfigHelper.MigrateFromObject(settings, "config\\config.xlsm", "config\\config_migrated.xlsm");

        }
    }
}
