using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Necrobot.Build
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var releaseSite = args[0];
            var buildFolder = args[1];
            //var releaseSite = @"D:\POGO\necrobot_fork\doc";
            //var buildFolder = @"D:\POGO\necrobot_fork\NecroBot";
            var assembly = $"{buildFolder}\\PoGo.NecroBot.CLI\\bin\\release\\NecroBot2.exe";

            var version = Assembly.LoadFile(assembly).GetName().Version.ToString();
            var antifactFolder = $"{releaseSite}\\releases\\{version}";
            if(Directory.Exists(antifactFolder))
            Directory.Delete(antifactFolder,true);

            Directory.CreateDirectory(antifactFolder);
            ZipFile.CreateFromDirectory($"{buildFolder}\\PoGo.NecroBot.CLI\\bin\\release", $"{antifactFolder}\\Necrobot.CLI.zip");
            ZipFile.CreateFromDirectory($"{buildFolder}\\PoGo.NecroBot.Window\\bin\\release", $"{antifactFolder}\\Necrobot.Win.zip");
            ZipFile.CreateFromDirectory($"{buildFolder}\\PoGo.NecroBot.GUI.Electron\\dist\\win-unpacked", $"{antifactFolder}\\Necrobot.Electron.GUI.zip");

            ReleasePage.GeneratePage($"{releaseSite}\\_posts\\release-template.md", $"{releaseSite}\\_posts\\{DateTime.Now:yyyy-MM-dd}-release-version-{version}.md", new ReleaseInfo()
            {
                Version = version,
                Changelogs = new List<string>() { "", "" },
                Downloads = new List<KeyValuePair<string, string>>() {

                new KeyValuePair<string, string>("Necrobot Console CLI",$"/releases/{version}/Necrobot.CLI.zip"),
                new KeyValuePair<string, string>("Necrobot Window GUI", $"/releases/{version}/Necrobot.Win.zip") ,
                new KeyValuePair<string, string>("Necrobot Electron GUI", $"/releases/{version}/Necrobot.Electron.GUI.zip") 
            },
            });
        }
    }
}
