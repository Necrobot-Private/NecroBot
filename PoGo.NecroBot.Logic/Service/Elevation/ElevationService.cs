using System;
using System.Collections.Generic;
using System.Linq;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Model;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public class ElevationService : IElevationService
    {
        private ElevationConfigContext _context = new ElevationConfigContext();
        private GlobalSettings _settings;
        
        private List<IElevationService> ElevationServiceQueue = new List<IElevationService>();
        public Dictionary<Type, DateTime> ElevationServiceBlacklist = new Dictionary<Type, DateTime>();

        public ElevationService(GlobalSettings settings)
        {
            _settings = settings;

            if (_settings.MapzenWalkConfig.UseMapzenWalk)
            {
                if (!string.IsNullOrEmpty(settings.MapzenWalkConfig.MapzenElevationApiKey))
                    ElevationServiceQueue.Add(new MapzenElevationService(settings));
            }

            if (_settings.GoogleWalkConfig.UseGoogleWalk)
            {
                if (!string.IsNullOrEmpty(settings.GoogleWalkConfig.GoogleElevationAPIKey))
                    ElevationServiceQueue.Add(new GoogleElevationService(settings));
            }

            ElevationServiceQueue.Add(new RandomElevationService(settings));
        }

        public bool IsElevationServiceBlacklisted(Type strategy)
        {
            if (!ElevationServiceBlacklist.ContainsKey(strategy))
                return false;

            DateTime now = DateTime.Now;
            DateTime blacklistExpiresAt = ElevationServiceBlacklist[strategy];
            if (blacklistExpiresAt < now)
            {
                // Blacklist expired
                ElevationServiceBlacklist.Remove(strategy);
                return false;
            }
            else
            {
                return true;
            }
        }

        public void BlacklistStrategy(Type strategy)
        {
            // Black list for 1 hour.
            ElevationServiceBlacklist[strategy] = DateTime.Now.AddHours(1);
        }

        public IElevationService GetService()
        {
            return ElevationServiceQueue.First(q => !IsElevationServiceBlacklisted(q.GetType()));
        }

        public async Task<double> GetElevation(double lat, double lng)
        {
            IElevationService service = GetService();
            
            if (service is RandomElevationService)
            {
                // Don't hit the database for random elevation service.
                return await service.GetElevation(lat, lng).ConfigureAwait(false);
            }

            ElevationLocation elevationLocation = await ElevationLocation.FindOrUpdateInDatabase(_context, lat, lng, service).ConfigureAwait(false);
            if (elevationLocation == null)
            {
                Logger.Write(
                    $"{service.GetServiceId()} response not reliable and will be blacklisted for one hour.",
                    LogLevel.Warning
                );
                BlacklistStrategy(service.GetType());

                Logger.Write(
                    $"Falling back to next elevation strategy: {GetService().GetServiceId()}.",
                    LogLevel.Warning
                );

                // After blacklisting, retry.
                return await service.GetElevation(lat, lng).ConfigureAwait(false);
            }

            return BaseElevationService.GetRandomElevation(elevationLocation.Altitude);
        }

        public string GetServiceId()
        {
            IElevationService service = GetService();
            return service.GetServiceId();
        }
    }
}