using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Model.Settings;
using System.Threading.Tasks;
using System.Net.Http;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public class GoogleResponse
    {
        public string Status { get; set; }
        public List<GoogleElevationResults> Results { get; set; }
    }

    public class GoogleElevationResults
    {
        public double Elevation { get; set; }
        public double Resolution { get; set; }
        public GoogleLocation Location { get; set; }
    }

    public class GoogleLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class GoogleElevationService : BaseElevationService
    {
        public GoogleElevationService(GlobalSettings settings) : base(settings)
        {
            if (!string.IsNullOrEmpty(settings.GoogleWalkConfig.GoogleElevationAPIKey))
                _apiKey = settings.GoogleWalkConfig.GoogleElevationAPIKey;
        }

        public override string GetServiceId()
        {
            return "Google Elevation Service";
        }

        public override async Task<double> GetElevationFromWebService(double lat, double lng)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return 0;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://maps.googleapis.com/maps/api/elevation/json?key={_apiKey}&locations={lat},{lng}";

                    var responseContent = await client.GetAsync(url).ConfigureAwait(false);
                    if (responseContent.StatusCode != HttpStatusCode.OK)
                        return 0;

                    var responseFromServer = await responseContent.Content.ReadAsStringAsync().ConfigureAwait(false);
                    GoogleResponse googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(responseFromServer);

                    if (googleResponse.Status == "OK" && googleResponse.Results != null &&
                        0 < googleResponse.Results.Count && googleResponse.Results[0].Elevation > -100)
                    return googleResponse.Results[0].Elevation;
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