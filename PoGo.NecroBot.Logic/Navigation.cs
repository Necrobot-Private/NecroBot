#region using directives

using GeoCoordinatePortable;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Interfaces.Configuration;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Strategies.Walk;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace PoGo.NecroBot.Logic
{
    public delegate void UpdatePositionDelegate(ISession session, double lat, double lng, double speed);
    public delegate void GetRouteDelegate(List<GeoCoordinate> points);

    public class Navigation
    {
        public IWalkStrategy WalkStrategy { get; set; }
        private readonly Client _client;
        private Random WalkingRandom = new Random();
        private List<IWalkStrategy> WalkStrategyQueue { get; set; }

        public Dictionary<Type, DateTime> WalkStrategyBlackList = new Dictionary<Type, DateTime>();
        public FortTargetEvent fortTargetEvent;

        private bool _GoogleWalk, _MapZenWalk, _YoursWalk, _GpxPathing, _AutoWalkAI;
        private string _GoogleAPI, _MapZenAPI;
        private double distance;
        private int _AutoWalkDist;

        public Navigation(Client client, ILogicSettings logicSettings)
        {
            _client = client;

            // Need these to recall useres preset walking vars at first load of Navigation.
            _GoogleWalk = logicSettings.UseGoogleWalk;
            _GoogleAPI = logicSettings.GoogleApiKey;
            _MapZenWalk = logicSettings.UseMapzenWalk;
            _MapZenAPI = logicSettings.MapzenTurnByTurnApiKey;
            _YoursWalk = logicSettings.UseYoursWalk;
            _GpxPathing = logicSettings.UseGpxPathing;

            _AutoWalkAI = logicSettings.AutoWalkAI;
            _AutoWalkDist = logicSettings.AutoWalkDist;

            InitializeWalkStrategies(logicSettings);
            WalkStrategy = GetStrategy(logicSettings);
        }

        public double VariantRandom(ISession session, double currentSpeed)
        {
            if (WalkingRandom.Next(1, 10) > 5)
            {
                if (WalkingRandom.Next(1, 10) > 5)
                {
                    var randomicSpeed = currentSpeed;
                    var max = session.LogicSettings.WalkingSpeedInKilometerPerHour +
                              session.LogicSettings.WalkingSpeedVariant;
                    randomicSpeed += WalkingRandom.NextDouble() * (0.02 - 0.001) + 0.001;

                    if (randomicSpeed > max)
                        randomicSpeed = max;

                    if (Math.Round(randomicSpeed, 2) != Math.Round(currentSpeed, 2))
                    {
                        session.EventDispatcher.Send(new HumanWalkingEvent
                        {
                            OldWalkingSpeed = currentSpeed,
                            CurrentWalkingSpeed = randomicSpeed
                        });
                    }
                    return randomicSpeed;
                }
                else
                {
                    var randomicSpeed = currentSpeed;
                    var min = session.LogicSettings.WalkingSpeedInKilometerPerHour -
                              session.LogicSettings.WalkingSpeedVariant;
                    randomicSpeed -= WalkingRandom.NextDouble() * (0.02 - 0.001) + 0.001;

                    if (randomicSpeed < min)
                        randomicSpeed = min;

                    if (Math.Round(randomicSpeed, 2) != Math.Round(currentSpeed, 2))
                    {
                        session.EventDispatcher.Send(new HumanWalkingEvent
                        {
                            OldWalkingSpeed = currentSpeed,
                            CurrentWalkingSpeed = randomicSpeed
                        });
                    }
                    return randomicSpeed;
                }
            }
            return currentSpeed;
        }

        public async Task Move(IGeoLocation targetLocation,
            Func<Task> functionExecutedWhileWalking,
            ISession session,
            CancellationToken cancellationToken, double customWalkingSpeed = 0.0)
        {
            // Need these to recall useres preset walking vars before bot continues to next POI.
            _GoogleWalk = session.LogicSettings.UseGoogleWalk;
            _GoogleAPI = session.LogicSettings.GoogleApiKey;
            _MapZenWalk = session.LogicSettings.UseMapzenWalk;
            _MapZenAPI = session.LogicSettings.MapzenTurnByTurnApiKey;
            _YoursWalk = session.LogicSettings.UseYoursWalk;
            _GpxPathing = session.LogicSettings.UseGpxPathing;

            _AutoWalkAI = session.LogicSettings.AutoWalkAI;
            _AutoWalkDist = session.LogicSettings.AutoWalkDist;

            distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude, session.Client.CurrentLongitude,
            targetLocation.Latitude, targetLocation.Longitude);

            cancellationToken.ThrowIfCancellationRequested();
            TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>().ThrowIfSwitchAccountRequested();

            // If the stretegies become bigger, create a factory for easy management

            //Logging.Logger.Write($"Navigation - Walking speed {customWalkingSpeed}");
            InitializeWalkStrategies(session.LogicSettings);
            WalkStrategy = GetStrategy(session.LogicSettings);
            await WalkStrategy.Walk(targetLocation, functionExecutedWhileWalking, session, cancellationToken, customWalkingSpeed).ConfigureAwait(false);
        }

        private void InitializeWalkStrategies(ILogicSettings logicSettings)
        {
            //AutoWalkAI code???
            if(_AutoWalkAI)
            { 
                if (distance >= _AutoWalkDist)
                {
                    if (_MapZenWalk == false && _MapZenAPI != "")
                    {
                        Logging.Logger.Write($"Distance to travel is > {_AutoWalkDist}m, switching to 'MapzenWalk'", Logging.LogLevel.Info, ConsoleColor.DarkYellow);
                        _YoursWalk = false;
                        _MapZenWalk = true;
                    }
                    if (_GoogleWalk == false && _GoogleAPI != "")
                    {
                        Logging.Logger.Write($"Distance to travel is > {_AutoWalkDist}m, switching to 'GoogleWalk'", Logging.LogLevel.Info, ConsoleColor.DarkYellow);
                        _YoursWalk = false;
                        _GoogleWalk = true;
                    }
                }
                else
                {
                    if (_GoogleWalk || _MapZenWalk)
                    {
                        string route = null;
                        try
                        {
                            route = fortTargetEvent.Route;
                        }
                        catch
                        {
                            route = "NecroBot Walk";
                        }
                        Logging.Logger.Write($"Distance to travel is < {_AutoWalkDist}m, switching back to '{route}'", Logging.LogLevel.Info, ConsoleColor.DarkYellow);
                    }
                }
            }

            WalkStrategyQueue = new List<IWalkStrategy>();

            // Maybe change configuration for a Navigation Type.
            if (logicSettings.DisableHumanWalking)
            {
                WalkStrategyQueue.Add(new FlyStrategy(_client));
            }

            if (_GpxPathing)
            {
                WalkStrategyQueue.Add(new HumanPathWalkingStrategy(_client));
            }

            if (_GoogleWalk)
            {
                WalkStrategyQueue.Add(new GoogleStrategy(_client));
            }

            if (_MapZenWalk)
            {
                WalkStrategyQueue.Add(new MapzenNavigationStrategy(_client));
            }

            if (_YoursWalk)
            {
                WalkStrategyQueue.Add(new YoursNavigationStrategy(_client));
            }

            WalkStrategyQueue.Add(new HumanStrategy(_client));
        }

        public bool IsWalkingStrategyBlacklisted(Type strategy)
        {
            if (!WalkStrategyBlackList.ContainsKey(strategy))
                return false;

            DateTime now = DateTime.Now;
            DateTime blacklistExpiresAt = WalkStrategyBlackList[strategy];
            if (blacklistExpiresAt < now)
            {
                // Blacklist expired
                WalkStrategyBlackList.Remove(strategy);
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
            WalkStrategyBlackList[strategy] = DateTime.Now.AddHours(1);
        }

        public IWalkStrategy GetStrategy(ILogicSettings logicSettings)
        {
            return WalkStrategyQueue.First(q => !IsWalkingStrategyBlacklisted(q.GetType()));
        }
    }
}
