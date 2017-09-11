#region using directives

#region using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using PoGo.NecroBot.Logic.Exceptions;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using TinyIoC;

#endregion

// ReSharper disable CyclomaticComplexity

#endregion

namespace PoGo.NecroBot.Logic.Utils
{
    public delegate void StatisticsDirtyDelegate();

    public class Statistics
    {
        private AccountConfigContext _context = new AccountConfigContext();
        private DateTime _initSessionDateTime = DateTime.Now;

        private StatsExport _exportStats;
        private string _playerName;
        public int TotalExperience;
        public int TotalItemsRemoved;
        public int TotalPokemons = 0;
        public int TotalPokemonEvolved = 0;
        public int TotalPokestops = 0;
        public int TotalPokemonTransferred;
        public int TotalStardust;
        public int LevelForRewards = -1;
        public bool isRandomTimeSet = false;
        public int newRandomSwitchTime = 1; // Initializing random switch time

        public StatsExport StatsExport => _exportStats;

        public void Dirty(Inventory inventory, ISession session)
        {
            _exportStats = GetCurrentInfo(session, inventory).Result;
            TotalStardust = inventory.GetStarDust();
            TinyIoCContainer.Current.Resolve<MultiAccountManager>().DirtyEventHandle(this);
            DirtyEvent?.Invoke();
            OnStatisticChanged(session);
        }

        public void OnStatisticChanged(ISession session)
        {
            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();
            if (MultipleBotConfig.IsMultiBotActive(session.LogicSettings, manager) && manager.AllowSwitch())
            {
                var config = session.LogicSettings.MultipleBotConfig;

                if (config.PokestopSwitch > 0 && config.PokestopSwitch <= TotalPokestops)
                {
                    session.CancellationTokenSource.Cancel();

                    //Activate switcher by pokestop
                    throw new ActiveSwitchByRuleException()
                    {
                        MatchedRule = SwitchRules.Pokestop,
                        ReachedValue = TotalPokestops
                    };
                }

                if (config.PokemonSwitch > 0 && config.PokemonSwitch <= TotalPokemons)
                {
                    session.CancellationTokenSource.Cancel();
                    //Activate switcher by Pokemon
                    throw new ActiveSwitchByRuleException()
                    {
                        MatchedRule = SwitchRules.Pokemon,
                        ReachedValue = TotalPokemons
                    };
                }

                if (config.EXPSwitch > 0 && config.EXPSwitch <= TotalExperience)
                {
                    session.CancellationTokenSource.Cancel();
                    //Activate switcher by EXP
                    throw new ActiveSwitchByRuleException()
                    {
                        MatchedRule = SwitchRules.EXP,
                        ReachedValue = TotalExperience
                    };
                }

                // When bot starts OR did the account switch by time, random time for Runtime has not been set. So we need to set it
                if (!isRandomTimeSet)
                {
                    Random random = new Random();
                    newRandomSwitchTime = config.RuntimeSwitch + random.Next((config.RuntimeSwitchRandomTime * -1), config.RuntimeSwitchRandomTime); //config.RuntimeSwitchRandomTime * -1, 0);
                    isRandomTimeSet = true;

                    Logger.Write($"Current Account will run for aprox: {newRandomSwitchTime} Min.",LogLevel.Info, ConsoleColor.Red);
                }

                var totalMin = (DateTime.Now - _initSessionDateTime).TotalMinutes;
                if (newRandomSwitchTime > 0 && newRandomSwitchTime <= totalMin)
                {
                    // Setup random time to false, so that next account generates new random runtime
                    isRandomTimeSet = false;

                    session.CancellationTokenSource.Cancel();
                    //Activate switcher by pokestop
                    throw new ActiveSwitchByRuleException()
                    {
                        MatchedRule = SwitchRules.Runtime,
                        ReachedValue = Math.Round(totalMin, 1)
                    };
                }
            }
        }

        public event StatisticsDirtyDelegate DirtyEvent;

        public string FormatRuntime()
        {
            return (DateTime.Now - _initSessionDateTime).ToString(@"dd\.hh\:mm\:ss");
        }

        public async Task<StatsExport> GetCurrentInfo(ISession session, Inventory inventory)
        {
            var stats = await inventory.GetPlayerStats().ConfigureAwait(false);
            StatsExport output = null;
            var stat = stats.FirstOrDefault();
            if (stat != null)
            {
                var ep = stat.NextLevelXp - stat.Experience;
                var time = Math.Round(ep / (TotalExperience / GetRuntime()), 2);
                var hours = 0.00;
                var minutes = 0.00;

                var TotXP = 0;

                for (int i = 0; i < stat.Level + 1; i++)
                {
                    TotXP = TotXP + Statistics.GetXpDiff(i);
                }

                if (double.IsInfinity(time) == false && time > 0)
                {
                    hours = Math.Truncate(TimeSpan.FromHours(time).TotalHours);
                    minutes = TimeSpan.FromHours(time).Minutes;
                }

                if (LevelForRewards == -1 || stat.Level >= LevelForRewards)
                {
                    if (session.LogicSettings.SkipCollectingLevelUpRewards)
                    {
                        Logger.Write("Current Lvl: " + stat.Level + ". Skipped collecting level up rewards.", LogLevel.Info);
                    }
                    else
                    {
                        LevelUpRewardsResponse Result = await inventory.GetLevelUpRewards(stat.Level).ConfigureAwait(false);

                        if (Result.ToString().ToLower().Contains("awarded_already"))
                            LevelForRewards = stat.Level + 1;

                        if (Result.ToString().ToLower().Contains("success"))
                        {
                            Logger.Write($"{session.Profile.PlayerData.Username} has leveled up: " + stat.Level, LogLevel.Info);
                            LevelForRewards = stat.Level + 1;

                            RepeatedField<ItemAward> items = Result.ItemsAwarded;
                            string Rewards = "";

                            if (items.Any<ItemAward>())
                            {
                                Logger.Write("- Received Items -", LogLevel.Info);
                                Rewards = "\nItems Recieved:";
                                foreach (ItemAward item in items)
                                {
                                    Logger.Write($"{item.ItemCount,2:#0}x {item.ItemId}'s", LogLevel.Info);
                                    Rewards += $"\n{item.ItemCount,2:#0}x {item.ItemId}'s";
                                }
                            }

                            if (session.LogicSettings.NotificationConfig.EnablePushBulletNotification)
                                await PushNotificationClient.SendNotification(session, $"{session.Profile.PlayerData.Username} has leveled up.", $"Trainer just reached level {stat.Level}{Rewards}", true).ConfigureAwait(false);
                        }
                    }
                }

                output = new StatsExport
                {
                    Level = stat.Level,
                    HoursUntilLvl = hours,
                    MinutesUntilLevel = minutes,
                    LevelXp = TotXP,
                    CurrentXp = stat.Experience,
                    PreviousXp = stat.PrevLevelXp,
                    LevelupXp = stat.NextLevelXp
                };
            }
            return output;
        }

        private object GetCurrentAccount()
        {
            var session = TinyIoCContainer.Current.Resolve<ISession>();
            return _context.Account.FirstOrDefault(a => session.Settings.Username == a.Username && session.Settings.AuthType == a.AuthType);
            //throw new NotImplementedException();
        }

        internal void Reset()
        {
            TotalExperience = 0;
            TotalItemsRemoved = 0;
            TotalPokemons = 0;
            TotalPokemonEvolved = 0;
            TotalPokestops = 0;
            TotalStardust = 0;
            TotalPokemonTransferred = 0;
            _initSessionDateTime = DateTime.Now;
            _exportStats = new StatsExport();
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(ISession ctx)
        {
            return await ctx.Inventory.GetLevelUpRewards(LevelForRewards).ConfigureAwait(false);
        }

        public double GetRuntime()
        {
            return (DateTime.Now - _initSessionDateTime).TotalSeconds / 3600;
        }

        public string GetTemplatedStats(string template, string xpTemplate)
        {
            var xpStats = string.Format(xpTemplate, _exportStats.Level, _exportStats.HoursUntilLvl,
                _exportStats.MinutesUntilLevel, _exportStats.CurrentXp - _exportStats.LevelXp, _exportStats.LevelupXp - _exportStats.LevelXp);

            return string.Format(template, _playerName, FormatRuntime(), xpStats, TotalExperience / GetRuntime(),
                TotalPokestops / GetRuntime(),
                TotalStardust, TotalPokemonTransferred, TotalItemsRemoved);
        }

        public static int GetXpDiff(int level)
        {
            if (level > 0 && level <= 40)
            {
                int[] xpTable =
                {
                    0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000,
                    10000, 10000, 10000, 10000, 15000, 20000, 20000, 20000, 25000, 25000,
                    50000, 75000, 100000, 125000, 150000, 190000, 200000, 250000, 300000, 350000,
                    500000, 500000, 750000, 1000000, 1250000, 1500000, 2000000, 2500000, 3000000, 5000000
                };
                return xpTable[level - 1];
            }
            return 0;
        }

        public void SetUsername(GetPlayerResponse profile)
        {
            _playerName = profile.PlayerData.Username ?? "";
        }
    }

    public class StatsExport
    {
        public long CurrentXp;
        public double HoursUntilLvl;
        public int Level;
        public int LevelXp;
        public long LevelupXp;
        public long PreviousXp;
        public double MinutesUntilLevel;
    }
}
