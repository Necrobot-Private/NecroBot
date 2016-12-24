using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Captcha Config", Description = "Set your Captcha settings.", ItemRequired = Required.DisallowNull)]

    public class CaptchaConfig
    {
        [DefaultValue("")]
        [MinLength(0)]
        [MaxLength(100)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        [ExcelConfig(Description = "Your 2Captcha Key", Position = 2)]
        public string TwoCaptchaKey = "";
    }
}
