using System;
using System.IO;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using System.Linq;

namespace PoGo.NecroBot.Logic.State
{
    public class SessionStats
    {
        const string DB_NAME = @"SessionStats.db";
        const string POKESTOP_STATS_COLLECTION = "PokeStopTimestamps";
        const string POKEMON_STATS_COLLECTION = "PokemonTimestamps";

        public int SnipeCount { get; set; }
        public DateTime LastSnipeTime { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsSnipping { get; internal set; }

        private ISession ownerSession;
       
        class PokeStopTimestamp
        {
            public Int64 Timestamp { get; set; }
        }

        class PokemonTimestamp
        {
            public Int64 Timestamp { get; set; }
        }

        DateTime lastPrintPokestopMessage = DateTime.Now;

        public bool SearchThresholdExceeds(ISession session, bool printMessage)
        {
            if (!session.LogicSettings.UsePokeStopLimit) return false;
            //if (_pokestopLimitReached || _pokestopTimerReached) return true;
            
            CleanOutExpiredStats();

            // Check if user defined max Pokestops reached
            var timeDiff = (DateTime.Now - session.Stats.StartTime);

            if (GetNumPokestopsInLast24Hours() >= session.LogicSettings.PokeStopLimit)
            {
                if (printMessage && lastPrintPokestopMessage.AddSeconds(60) < DateTime.Now)
                {
                    lastPrintPokestopMessage = DateTime.Now;
                    session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.PokestopLimitReached)
                    });
                }
                //_pokestopLimitReached = true;
                return true;
            }

            // Check if user defined time since start reached
            else if (timeDiff.TotalSeconds >= session.LogicSettings.PokeStopLimitMinutes * 60)
            {
                session.EventDispatcher.Send(new ErrorEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.PokestopTimerReached)
                });

                //_pokestopTimerReached = true;
                return true;
            }

            return false; // Continue running
        }


        DateTime lastPrintCatchMessage = DateTime.Now;

        public bool CatchThresholdExceeds(ISession session, bool printMessage = true)
        {
            if (!session.LogicSettings.UseCatchLimit) return false;
            
            CleanOutExpiredStats();

            var timeDiff = (DateTime.Now - session.Stats.StartTime);

            // Check if user defined max AMOUNT of Catches reached
            if (GetNumPokemonsInLast24Hours() >= session.LogicSettings.CatchPokemonLimit)
            {
                if (printMessage && lastPrintCatchMessage.AddSeconds(60) < DateTime.Now)
                {
                    lastPrintCatchMessage = DateTime.Now;
                    session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.CatchLimitReached)
                    });
                }
                // _catchPokemonLimitReached = true;
                return true;
            }

            // Check if user defined TIME since start reached
            else if (timeDiff.TotalSeconds >= session.LogicSettings.CatchPokemonLimitMinutes * 60)
            {
                session.EventDispatcher.Send(new ErrorEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.CatchTimerReached)
                });

                //_catchPokemonTimerReached = true;
                return true;
            }

            return false;
        }

        public bool IsPokestopLimit(ISession session)
        {
            if (!session.LogicSettings.UsePokeStopLimit) return false;
            
            CleanOutExpiredStats();

            if (GetNumPokestopsInLast24Hours() >= session.LogicSettings.PokeStopLimitMinutes)
                return true;
            //TODO - Other logic should come here, but I don't think we need
            return false;
        }

        public SessionStats(ISession session)
        {
            StartTime = DateTime.Now;
            ownerSession = session;
        }
        
        private static string GetUsername(ISession session)
        {
            return session.Settings.Username;
        }

        private static string GetDBPath(ISession session, string username)
        {
            var path = Path.Combine(session.LogicSettings.ProfileConfigPath, username);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, DB_NAME);
            return path;
        }

        public void AddPokestopTimestamp(Int64 ts)
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            var db = manager.GetDbContext();
            var existing = db.PokestopTimestamp.Where(t => t.Timestamp == ts).FirstOrDefault();
            if (existing == null)
            {
                var currentAccount = manager.GetCurrentAccount();

                var stat = new Model.PokestopTimestamp
                {
                    Timestamp = ts,
                    Account = manager.GetCurrentAccount()
                };
                db.PokestopTimestamp.Add(stat);
                db.SaveChanges();
            }
        }

        public void AddPokemonTimestamp(Int64 ts)
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            var db = manager.GetDbContext();
            var existing = db.PokemonTimestamp.Where(t => t.Timestamp == ts).FirstOrDefault();
            if (existing == null)
            {
                var currentAccount = manager.GetCurrentAccount();

                var stat = new Model.PokemonTimestamp
                {
                    Timestamp = ts,
                    Account = manager.GetCurrentAccount()
                };
                db.PokemonTimestamp.Add(stat);
                db.SaveChanges();
            }
        }

        public void CleanOutExpiredStats()
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            var db = manager.GetDbContext();
            long TSminus24h = DateTime.Now.AddHours(-24).Ticks;
            var pokestopTimestampsToDelete = db.PokestopTimestamp.Where(t => t.Account == manager.GetCurrentAccount() && t.Timestamp < TSminus24h);
            db.PokestopTimestamp.RemoveRange(pokestopTimestampsToDelete);

            var pokemonTimestampsToDelete = db.PokemonTimestamp.Where(t => t.Account == manager.GetCurrentAccount() && t.Timestamp < TSminus24h);
            db.PokemonTimestamp.RemoveRange(pokemonTimestampsToDelete);
            db.SaveChanges();
        }

        public int GetNumPokestopsInLast24Hours()
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            var db = manager.GetDbContext();
            var TSminus24h = DateTime.Now.AddHours(-24).Ticks;
            return db.PokestopTimestamp.Count(s => manager.GetCurrentAccount() == s.Account && s.Timestamp >= TSminus24h);
        }

        public int GetNumPokemonsInLast24Hours()
        {
            var manager = TinyIoC.TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            var db = manager.GetDbContext();
            var TSminus24h = DateTime.Now.AddHours(-24).Ticks;
            return db.PokemonTimestamp.Count(s => manager.GetCurrentAccount() == s.Account && s.Timestamp >= TSminus24h);
        }
    }
}
