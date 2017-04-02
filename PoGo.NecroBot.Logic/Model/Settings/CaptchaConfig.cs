using System.ComponentModel;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Captcha Config", Description = "Setup captcha config", ItemRequired = Required.DisallowNull)]
    public class CaptchaConfig : BaseConfig
    {
        public CaptchaConfig() : base()
        {
        }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [NecrobotConfig(Position = 1, Description = "Display captchas on browser and allow resolving manually")]
        public bool AllowManualCaptchaResolve { get; set; }

        [DefaultValue(120)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        [NecrobotConfig(Position = 2, Description = "Number of seconds bot will wait for you to resolve captcha, if after the time set and captcha havent resolved yet, the bot will continue ")]
        public int ManualCaptchaTimeout { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [NecrobotConfig(Position = 3, Description = "Play an alert sound when you receive a captcha")]
        public bool PlaySoundOnCaptcha { get; set; }

        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 4, Description = "Display captchas on top of your screen")]
        public bool DisplayOnTop { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 5, Description = "Enable Auto captcha solving with 2Captcha")]
        public bool Enable2Captcha { get; set; }

        [DefaultValue(3)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        [NecrobotConfig(Position = 6, Description = "Number of times bot will try to resolve captchas automatically")]
        public int AutoCaptchaRetries { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        [NecrobotConfig(Position = 7, Description = "API Key to use 2Captcha")]
        public string TwoCaptchaAPIKey { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 8, Description = "Enable Auto captcha solving with Anti-Captcha")]
        public bool EnableAntiCaptcha { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 9, Description = "API Key to use Anti-Captcha")]
        public string AntiCaptchaAPIKey { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 10, Description = "Proxy host to be used by captcha service")]
        public string ProxyHost { get; set; }

        [DefaultValue(3128)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 11, Description = "Proxy port to be used by captcha service")]
        public int ProxyPort { get; set; }

        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 12, Description = "Enable Auto captcha solving with CaptchaSolutions.com")]
        public bool EnableCaptchaSolutions { get; set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 13, Description = "API Key to use CaptchaSolutions")]
        public string CaptchaSolutionAPIKey { get;  set; }

        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 14, Description = "Secret Key to use for CaptchaSolutions")]
        public string CaptchaSolutionsSecretKey { get; set; }

        [DefaultValue(120)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        [NecrobotConfig(Position = 15, Description = "Timeout for auto captcha solving")]
        public int AutoCaptchaTimeout { get; set; }
        
    }
}