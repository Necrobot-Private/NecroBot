using PoGo.NecroBot.Logic.Model.Settings;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PoGo.NecroBot.Logic.Model
{
    public partial class Account
    {
        public long Id { get; set; }
        public AuthType AuthType { get; set; }
        [Required]
        public string Username { get; set; }
        public bool AutoExitBotIfAccountFlagged { get; set; }
        public double AccountLatitude { get; set; }
        public double AccountLongitude { get; set; }
        public bool AccountActive { get; set; }
        [Required]
        public string Password { get; set; }
        public double? RuntimeTotal { get; set; }
        public long? LastRuntimeUpdatedAt { get; set; }
        public long? ReleaseBlockTime { get; set; }
        public string Nickname { get; set; }
        public long? LoggedTime { get; set; }
        public long? Level { get; set; }
        public string LastLogin { get; set; }
        public long? LastLoginTimestamp { get; set; }
        public long? Stardust { get; set; }
        public long? CurrentXp { get; set; }
        public long? PrevLevelXp { get; set; }
        public long? NextLevelXp { get; set; }
        public long? IsRunning { get; set; }
        public ICollection<PokemonTimestamp> PokemonTimestamp { get; set; }
        public ICollection<PokestopTimestamp> PokestopTimestamp { get; set; }
        private GlobalSettings _globalSettings { get; set; }
    }

    public partial class Account : INotifyPropertyChanged
    {
        public Account()
        {
            PokemonTimestamp = new HashSet<PokemonTimestamp>();
            PokestopTimestamp = new HashSet<PokestopTimestamp>();
        }

        public Account(AuthConfig item)
        {
            AuthType = item.AuthType;
            Password = item.Password;
            Username = item.Username;
            AutoExitBotIfAccountFlagged = item.AutoExitBotIfAccountFlagged;
            AccountLatitude = item.AccountLatitude;
            AccountLongitude = item.AccountLongitude;
            AccountActive = item.AccountActive;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetRuntime()
        {
            if (RuntimeTotal.HasValue)
            {
                var seconds = (int)(RuntimeTotal.Value * 60);
                var duration = new TimeSpan(0, 0, seconds);

                return duration.ToString(@"dd\:hh\:mm\:ss");
            }

            return null;
        }

        public string ExperienceInfo
        {
            get
            {
                if (!CurrentXp.HasValue || !PrevLevelXp.HasValue || !NextLevelXp.HasValue)
                    return null;

                int percentComplete = 0;
                double xp = CurrentXp.Value - PrevLevelXp.Value;
                double levelXp = NextLevelXp.Value - PrevLevelXp.Value;

                if (levelXp > 0)
                    percentComplete = (int)Math.Floor(xp / levelXp * 100);
                return $"{xp}/{levelXp} ({percentComplete}%)";
            }
        }
    }
}
