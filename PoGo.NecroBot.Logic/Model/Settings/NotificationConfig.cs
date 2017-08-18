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
        [NecroBotConfig(Position = 1, Description = "Enable Pushbullet Notifications")]
        public bool EnablePushBulletNotification { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecroBotConfig(Position = 2, Description = "Enable Email Notifications")]
        public bool EnableEmailNotification { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecroBotConfig(Position = 3, Description = "API Key to connect to pushbullet (Go to pushbullet.com to get a key)")]
        public string PushBulletApiKey { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecroBotConfig(Position = 4, Description = "Gmail Address to send emails to")]
        public string GmailUsername { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        [NecroBotConfig(Position = 5, Description = "Gmail Password")]
        public string GmailPassword { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        [NecroBotConfig(Position = 6, Description = "List of email addresses allowed to recieve notificaitons")]
        public string Recipients { get; set; }
    }
}