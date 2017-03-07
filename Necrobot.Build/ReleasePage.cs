using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Necrobot.Build
{
    public class ReleasePage
    {
        public static void GeneratePage(string temlate, string output, ReleaseInfo info)
        {
            var inputText = File.ReadAllText(temlate);

            string outputHtml = SmartFormat.Smart.Format(inputText, info);
            outputHtml = outputHtml.Replace("2017-01-01 00:00", DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
            Console.WriteLine(outputHtml);
            File.WriteAllText(output, outputHtml);
        }
    }
}
