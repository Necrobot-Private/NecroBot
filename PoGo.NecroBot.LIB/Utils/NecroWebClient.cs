using System;
using System.Net;

namespace PoGo.NecroBot.LIB.Utils
{
    public class NecroWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);
            w.Timeout = 5000;
            return w;
        }
    }
}