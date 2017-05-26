using PoGo.NecroBot.Logic.Model.Settings;
using System;
using System.ComponentModel;

namespace PoGo.NecroBot.Logic.Model
{
    public class BotAccount : AuthConfig, INotifyPropertyChanged
    {
        public bool IsRunning { get; set; }
        public BotAccount() { }
        public BotAccount(AuthConfig item)
        {
            AuthType = item.AuthType;
            Password = item.Password;
            Username = item.Username;
        }

        // AutoId will be automatically incremented.
        public int Id { get; set; }
        public string Nickname { get; set; }
        public DateTime LoggedTime { get; set; }
        public int Level { get; set; }
        public string LastLogin { get; set; }
        public long LastLoginTimestamp { get; set; }
        public int Stardust { get; set; }
        public long CurrentXp { get; set; }
        public long PrevLevelXp { get; set; }
        public long NextLevelXp { get; set; }
        
        public string ExperienceInfo
        {
            get
            {
                int percentComplete = 0;
                double xp = CurrentXp - PrevLevelXp;
                double levelXp = NextLevelXp - PrevLevelXp;

                if (levelXp > 0)
                    percentComplete = (int)Math.Floor(xp / levelXp * 100);
                return $"{xp}/{levelXp} ({percentComplete}%)";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetRuntime()
        {
            var seconds = (int)((double)RuntimeTotal * 60);
            var duration = new TimeSpan(0, 0, seconds);

            return duration.ToString(@"dd\:hh\:mm\:ss");
        }
    }
}
