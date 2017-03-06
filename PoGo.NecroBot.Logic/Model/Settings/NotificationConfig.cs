using System.ComponentModel;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class NotificationConfig : BaseConfig
    {
        public NotificationConfig() : base()
        {
        }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecrobotConfig(Position = 1, Description = "Enable pushbullet notification")]
        public bool EnablePushBulletNotification { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecrobotConfig(Position = 2, Description = "Enable email notification")]
        public bool EnableEmailNotification { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecrobotConfig(Position = 3, Description = "API Key to connect to pushbullet - go to pushbullet.com to get one")]
        public string PushBulletApiKey { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 4, Description = "Gmail email address to use to send email")]
        public string GmailUsername { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        [NecrobotConfig(Position = 5, Description = "Gmail password")]
        public string GmailPassword { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        [NecrobotConfig(Position = 6, Description = "List of email address to recieve notificaitons")]
        public string Recipients { get; set; }
    }
}