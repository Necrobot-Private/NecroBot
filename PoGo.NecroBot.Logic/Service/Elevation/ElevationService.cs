using Caching;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Service.Elevation
{
    public class ElevationService
    {
        private ISession _session;
        LRUCache<string, double> cache = new LRUCache<string, double>(capacity: 500);

        private List<IElevationService> ElevationServiceQueue = new List<IElevationService>();
        public Dictionary<Type, DateTime> ElevationServiceBlacklist = new Dictionary<Type, DateTime>();

        public ElevationService(ISession session)
        {
            _session = session;

            if (!string.IsNullOrEmpty(session.LogicSettings.MapzenElevationApiKey))
                ElevationServiceQueue.Add(new MapzenElevationService(session, cache));

            ElevationServiceQueue.Add(new MapQuestElevationService(session, cache));

            if (!string.IsNullOrEmpty(session.LogicSettings.GoogleElevationApiKey))
                ElevationServiceQueue.Add(new GoogleElevationService(session, cache));

            ElevationServiceQueue.Add(new RandomElevationService(session, cache));
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

        public double GetElevation(double lat, double lng)
        {
            IElevationService service = GetService();
            double elevation = service.GetElevation(lat, lng);
            if (elevation == 0 || elevation < -100)
            {
                // Error getting elevation so just return 0.
                Logging.Logger.Write($"{service.GetServiceId()} response not reliable: {elevation.ToString()}, and will be blacklisted for one hour.", Logging.LogLevel.Warning);
                BlacklistStrategy(service.GetType());

                Logging.Logger.Write($"Falling back to next elevation strategy: {GetService().GetServiceId()}.", Logging.LogLevel.Warning);
                
                // After blacklisting, retry.
                return GetElevation(lat, lng);
            }

            return elevation;
        }
    }


}
