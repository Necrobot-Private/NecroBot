using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using PokemonGo.RocketAPI.Helpers;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(
         Title = "Device Config",
         Description = "Set your device settings (set \"DevicePackageName\" to \"random\" for auto-generated device). Set \"DevicePlatform\" to \"android\" or \"ios\".",
         ItemRequired = Required.DisallowNull
     )]
    public class DeviceConfig
    {
        internal enum DevicePlatformType
        {
            android,
            ios
        }

        [DefaultValue("ios")]
        [EnumDataType(typeof(DevicePlatformType))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string DevicePlatform = "ios";

        [DefaultValue("custom")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string DevicePackageName = "custom";

        [DefaultValue("2d207c0f186091c04abc7ff706a985ee")]
        [MinLength(16)]
        [MaxLength(32)]
        [RegularExpression(@"^[0-9A-Fa-f]+$")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        public string DeviceId = DeviceInfoHelper.GetRandomIosDevice().DeviceId;

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 4)]
        public string AndroidBoardName;

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public string AndroidBootloader;

        [DefaultValue("Apple")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 6)]
        public string DeviceBrand = "Apple";

        [DefaultValue("iPhone")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 7)]
        public string DeviceModel = "iPhone";

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 8)]
        public string DeviceModelIdentifier;

        [DefaultValue("iPhone9,3")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 9)]
        public string DeviceModelBoot = "iPhone9,3";

        [DefaultValue("Apple")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 10)]
        public string HardwareManufacturer = "Apple";

        [DefaultValue("D101AP")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 11)]
        public string HardwareModel = "D101AP";

        [DefaultValue("iOS")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 12)]
        public string FirmwareBrand = "iOS";

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        public string FirmwareTags;

        [DefaultValue("11.1.0")]
        [MinLength(0)]
        [MaxLength(32)]
        [RegularExpression(@"[a-zA-Z0-9_\-\.\s]")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 14)]
        public string FirmwareType = "11.1.0";

        [DefaultValue(null)]
        [MinLength(0)]
        [MaxLength(128)]
        [RegularExpression(@"[[a-zA-Z0-9_\-\/\.\:]")]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 15)]
        public string FirmwareFingerprint;

        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 16)]
        public bool UseRandomDeviceId;

        public static string GetDeviceId(string username)
        {
            Directory.CreateDirectory($"config\\{username}");
            string keyFile = $"config\\{username}\\device.id";

            if (File.Exists(keyFile)) return File.ReadAllText(keyFile);

            string hashUsername = "";
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(username + Path.GetRandomFileName()));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                hashUsername =  sb.ToString();
            }
            File.WriteAllText(keyFile, hashUsername);
            return hashUsername;
        }
    }
}