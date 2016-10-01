using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;
using PoGo.NecroBot.Logic.State;
using System.Threading;

namespace PoGo.NecroBot.Logic.Tasks
{
    public partial class HumanWalkSnipeTask
    {
        private static string ip;

        private static Task taskDataLive;
        public class FastPokemapItem
        {
            public class Lnglat
            {
                public string type { get; set; }
                public List<double> coordinates { get; set; }
            }

            public string _id { get; set; }
            public string pokemon_id { get; set; }
            public string encounter_id { get; set; }
            public string spawn_id { get; set; }
            public DateTime expireAt { get; set; }
            public int __v { get; set; }
            public Lnglat lnglat { get; set; }
        }

        private static string GetIP()
        {

            if (string.IsNullOrEmpty(ip))
            {
                var client = new HttpClient();
                var task = client.GetStringAsync("http://checkip.dyndns.org");
                task.Wait();
                ip = task.Result.Split(':')[1].Trim();
            }
            return ip;

        }
        
        public static async Task StartFastPokemapAsync(ISession session , CancellationToken cancellationToken)
        {
            var scanOffset = session.LogicSettings.HumanWalkingSnipeSnipingScanOffset;

            var offset = 0.025;

            var step = offset / 5;
            while (true)
            {
                await Task.Delay(30 * 1000);//sleep for 30 sec
                Logger.Write("Overkill fastpokemap to collect data :))");
                double lat = session.Client.CurrentLatitude;
                double lng = session.Client.CurrentLongitude;

                List<Task<List<FastPokemapItem>>> task = new List<Task<List<FastPokemapItem>>>();
                for (var x = -offset; x <= offset;)
                {
                    for (var y = -offset; y <= offset;)
                    {
                        try {
                            var scanLat = lat + x;
                            var scanLng = lng + y;
                            string scanurl = $"https://cache.fastpokemap.se/?key=2fe7ce70-90b8-460a-bffb-d7f3b4b74cc2&ts=57c9d27c&compute={GetIP()}&lat={scanLat}&lng={scanLng}";

                            var json = await DownloadContent(scanurl);
                            var data = JsonConvert.DeserializeObject<List<FastPokemapItem>>(json);
                            List<SnipePokemonInfo> chunk = new List<SnipePokemonInfo>();
                            foreach (var item in data)
                            {
                                var pItem = Map(item);
                                if (pItem != null && pItem.Id > 0)
                                {
                                    chunk.Add(pItem);
                                }
                            }
                            PostProcessDataFetched(chunk, false);
                        }
                        catch {
                        }
                        finally { y += step; };
                    }
                    x += step;
                }
            }
        }

        private static void StartAsyncPollingTask(ISession session, CancellationToken cancellationToken)
        {
            if (!session.LogicSettings.HumanWalkingSnipeUseFastPokemap) return;

            if (taskDataLive != null && !taskDataLive.IsCompleted) return;
            taskDataLive = Task.Run(() =>  
            {
				while(true)
                {
                    try
                    {
                        //cancellationToken.ThrowIfCancellationRequested();
                        var lat = _session.Client.CurrentLatitude;
                        var lng = _session.Client.CurrentLongitude;
                        var api = $"https://api.fastpokemap.se/?key=allow-all&ts=0&lat={lat}&lng={lng}";
                        string content =  DownloadContent(api).Result;
                        Task.Delay(1 * 60 * 1000).Wait();
                    }
                    catch
                    {
                        
                    }
                    
                }
            });
        }

        private static SnipePokemonInfo Map(FastPokemapItem result)
        {
            return new SnipePokemonInfo()
            {
                Latitude = result.lnglat.coordinates[1],
                Longitude = result.lnglat.coordinates[0],
                Id = GetId(result.pokemon_id),
                ExpiredTime = result.expireAt.ToLocalTime(),
                Source = "Fastpokemap"
            };
        }

        public static int GetId(string name)
        {
            var t = name[0];
            var realName = new StringBuilder(name.ToLower());
            realName[0] = t;
            try
            {
                var p = (PokemonId)Enum.Parse(typeof(PokemonId), realName.ToString());
                return (int)p;
            }
            catch (Exception)
            {

            }
            return 0;
        }

        private static async Task<string> DownloadContent(string url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("origin", "https://fastpokemap.se");
            request.Headers.Add("authority", "cache.fastpokemap.se");

            string result = "";

            using (HttpClient client = new HttpClient())
            {

                try
                {
                    var task = await client.SendAsync(request);
                    result = await task.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {

                }
            }

            return result;
        }

        private static async Task<List<SnipePokemonInfo>> FetchFromFastPokemap(double lat, double lng)
        {
            List<SnipePokemonInfo> results = new List<SnipePokemonInfo>();
            if (!_setting.HumanWalkingSnipeUseFastPokemap) return results;

            //var startFetchTime = DateTime.Now;

            try
            {
                string key = "2fe7ce70-90b8-460a-bffb-d7f3b4b74cc2";//allow-all
                string ts = "57c9d27c";

                string url = $"https://cache.fastpokemap.se/?key={key}&ts={ts}&compute={GetIP()}&lat={lat}&lng={lng}";
                
                var json = await DownloadContent(url);
                var data = JsonConvert.DeserializeObject<List<FastPokemapItem>>(json);
                foreach (var item in data)
                {
                    var pItem = Map(item);
                    if (pItem != null && pItem.Id > 0)
                    {
                        results.Add(pItem);
                    }
                }
            }
            catch (Exception)
            {
                Logger.Write("Error loading data fastpokemap", LogLevel.Error, ConsoleColor.DarkRed);
            }

            //var endFetchTime = DateTime.Now;
            //Logger.Write($"FetchFromFastPokemap spent {(endFetchTime - startFetchTime).TotalSeconds} seconds", LogLevel.Info, ConsoleColor.White);
            return results;
        }
    }
}
