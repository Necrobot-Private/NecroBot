using System;
using System.Net;
using Newtonsoft.Json.Linq;
using PoGo.NecroBot.Logic.Model.Settings;
using System.Threading.Tasks;
using System.Net.Http;
using PoGo.NecroBot.Logic.Exceptions;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public class MapzenElevationService : BaseElevationService
    {
        public MapzenElevationService(GlobalSettings settings) : base(settings)
        {
            if (!string.IsNullOrEmpty(settings.MapzenWalkConfig.MapzenElevationApiKey))
                _apiKey = settings.MapzenWalkConfig.MapzenElevationApiKey;
        }

        public override string GetServiceId()
        {
            return "Mapzen Elevation Service";
        }

        public override async Task<double> GetElevationFromWebService(double lat, double lng)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return 0;
            
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://elevation.mapzen.com/height?json=" + "{\"shape\":[{\"lat\":" + lat + ",\"lon\":" + lng + "}]}" + $"&api_key={_apiKey}";

                    var responseContent = await client.GetAsync(url).ConfigureAwait(false);
                    if (responseContent.StatusCode != HttpStatusCode.OK)
                        return 0;

                    var responseFromServer = await responseContent.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JObject jsonObj = JObject.Parse(responseFromServer);

                    JArray heights = (JArray)jsonObj["height"];
                    return (double)heights[0];
                }
                catch (ActiveSwitchByRuleException ex)
                {
                    throw ex;
                }
                catch (Exception)
                {
                    // If we get here for any reason, then just drop down and return 0. Will cause this elevation service to be blacklisted.
                }
            }
             
            return 0;
        }
    }
}