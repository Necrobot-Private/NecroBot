using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Necrobot.Build
{
    public class ReleaseInfo
    {
        public string Version { get; set; }
        public string Date { get; set; }
        public List<string> Changelogs { get; set; }

        public List<KeyValuePair<string, string>> Downloads { get; set; }

        public string ChangeLogsText
        {
            get {
                return string.Join("\r\n", this.Changelogs.Select(x=>"- " + x));
            }

        }
    }
}
