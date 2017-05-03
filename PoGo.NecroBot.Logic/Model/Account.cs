using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoGo.NecroBot.Logic.Model
{
    public partial class Account
    {
        public long Id { get; set; }
        public long AuthType { get; set; }
        [Required]
        public string Username { get; set; }
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
    }
}
