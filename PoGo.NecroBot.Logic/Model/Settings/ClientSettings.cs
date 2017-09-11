using System;
using PoGo.NecroBot.Logic.Service.Elevation;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class ClientSettings : ISettings
    {
        // Never spawn at the same position.
        private readonly Random _rand = new Random();

        private readonly GlobalSettings _settings;
        private readonly IElevationService _elevationService;

        public ClientSettings(GlobalSettings settings, IElevationService elevationService)
        {
            _settings = settings;
            _elevationService = elevationService;
        }
        
        #region Auth Config Values

        public bool UseProxy
        {
            get { return _settings.Auth.ProxyConfig.UseProxy; }
            set { _settings.Auth.ProxyConfig.UseProxy = value; }
        }

        public string UseProxyHost
        {
            get { return _settings.Auth.ProxyConfig.UseProxyHost; }
            set { _settings.Auth.ProxyConfig.UseProxyHost = value; }
        }

        public string UseProxyPort
        {
            get { return _settings.Auth.ProxyConfig.UseProxyPort; }
            set { _settings.Auth.ProxyConfig.UseProxyPort = value; }
        }

        public bool UseProxyAuthentication
        {
            get { return _settings.Auth.ProxyConfig.UseProxyAuthentication; }
            set { _settings.Auth.ProxyConfig.UseProxyAuthentication = value; }
        }

        public string UseProxyUsername
        {
            get { return _settings.Auth.ProxyConfig.UseProxyUsername; }
            set { _settings.Auth.ProxyConfig.UseProxyUsername = value; }
        }

        public string UseProxyPassword
        {
            get { return _settings.Auth.ProxyConfig.UseProxyPassword; }
            set { _settings.Auth.ProxyConfig.UseProxyPassword = value; }
        }

        public string GoogleRefreshToken
        {
            get { return null; }
            set { GoogleRefreshToken = null; }
        }
        
        AuthType ISettings.AuthType
        {
            get { return _settings.Auth.CurrentAuthConfig.AuthType; }

            set { _settings.Auth.CurrentAuthConfig.AuthType = value; }
        }

        string ISettings.Username
        {
            get { return _settings.Auth.CurrentAuthConfig.Username; }

            set { _settings.Auth.CurrentAuthConfig.Username = value; }
        }

        string ISettings.Password
        {
            get { return _settings.Auth.CurrentAuthConfig.Password; }

            set { _settings.Auth.CurrentAuthConfig.Password = value; }
        }

        bool ISettings.AutoExitBotIfAccountFlagged
        {
            get { return _settings.Auth.CurrentAuthConfig.AutoExitBotIfAccountFlagged; }

            set { _settings.Auth.CurrentAuthConfig.AutoExitBotIfAccountFlagged = value; }
        }

        double ISettings.AccountLatitude
        {
            get { return _settings.Auth.CurrentAuthConfig.AccountLatitude; }
            set { _settings.Auth.CurrentAuthConfig.AccountLatitude = value; }
        }

        double ISettings.AccountLongitude
        {
            get { return _settings.Auth.CurrentAuthConfig.AccountLongitude; }
            set { _settings.Auth.CurrentAuthConfig.AccountLongitude = value; }
        }

        bool ISettings.AccountActive
        {
            get { return _settings.Auth.CurrentAuthConfig.AccountActive; }
            set { _settings.Auth.CurrentAuthConfig.AccountActive = value; }
        }

        #endregion Auth Config Values

        #region Device Config Values

        public string DevicePlatform
        {
            get { return _settings.Auth.DeviceConfig.DevicePlatform; }
            set { _settings.Auth.DeviceConfig.DevicePlatform = value; }
        }

        string DevicePackageName
        {
            get { return _settings.Auth.DeviceConfig.DevicePackageName; }
            set { _settings.Auth.DeviceConfig.DevicePackageName = value; }
        }

        string ISettings.DeviceId
        {
            get { return _settings.Auth.DeviceConfig.DeviceId; }
            set { _settings.Auth.DeviceConfig.DeviceId = value; }
        }

        string ISettings.AndroidBoardName
        {
            get { return _settings.Auth.DeviceConfig.AndroidBoardName; }
            set { _settings.Auth.DeviceConfig.AndroidBoardName = value; }
        }

        string ISettings.AndroidBootloader
        {
            get { return _settings.Auth.DeviceConfig.AndroidBootloader; }
            set { _settings.Auth.DeviceConfig.AndroidBootloader = value; }
        }

        string ISettings.DeviceBrand
        {
            get { return _settings.Auth.DeviceConfig.DeviceBrand; }
            set { _settings.Auth.DeviceConfig.DeviceBrand = value; }
        }

        string ISettings.DeviceModel
        {
            get { return _settings.Auth.DeviceConfig.DeviceModel; }
            set { _settings.Auth.DeviceConfig.DeviceModel = value; }
        }

        string ISettings.DeviceModelIdentifier
        {
            get { return _settings.Auth.DeviceConfig.DeviceModelIdentifier; }
            set { _settings.Auth.DeviceConfig.DeviceModelIdentifier = value; }
        }

        string ISettings.DeviceModelBoot
        {
            get { return _settings.Auth.DeviceConfig.DeviceModelBoot; }
            set { _settings.Auth.DeviceConfig.DeviceModelBoot = value; }
        }

        string ISettings.HardwareManufacturer
        {
            get { return _settings.Auth.DeviceConfig.HardwareManufacturer; }
            set { _settings.Auth.DeviceConfig.HardwareManufacturer = value; }
        }

        string ISettings.HardwareModel
        {
            get { return _settings.Auth.DeviceConfig.HardwareModel; }
            set { _settings.Auth.DeviceConfig.HardwareModel = value; }
        }

        string ISettings.FirmwareBrand
        {
            get { return _settings.Auth.DeviceConfig.FirmwareBrand; }
            set { _settings.Auth.DeviceConfig.FirmwareBrand = value; }
        }

        string ISettings.FirmwareTags
        {
            get { return _settings.Auth.DeviceConfig.FirmwareTags; }
            set { _settings.Auth.DeviceConfig.FirmwareTags = value; }
        }

        string ISettings.FirmwareType
        {
            get { return _settings.Auth.DeviceConfig.FirmwareType; }
            set { _settings.Auth.DeviceConfig.FirmwareType = value; }
        }

        string ISettings.FirmwareFingerprint
        {
            get { return _settings.Auth.DeviceConfig.FirmwareFingerprint; }
            set { _settings.Auth.DeviceConfig.FirmwareFingerprint = value; }
        }

        #endregion Device Config Values

        double ISettings.DefaultLatitude
        {
            get
            {
                return _settings.LocationConfig.DefaultLatitude + _rand.NextDouble() *
                       ((double) _settings.LocationConfig.MaxSpawnLocationOffset / 111111);
            }

            set { _settings.LocationConfig.DefaultLatitude = value; }
        }

        double ISettings.DefaultLongitude
        {
            get
            {
                return _settings.LocationConfig.DefaultLongitude +
                       _rand.NextDouble() *
                       ((double) _settings.LocationConfig.MaxSpawnLocationOffset / 111111 /
                        Math.Cos(_settings.LocationConfig.DefaultLatitude));
            }

            set { _settings.LocationConfig.DefaultLongitude = value; }
        }

        double ISettings.DefaultAltitude
        {
            get
            {
                return LocationUtils.GetElevation(_elevationService, _settings.LocationConfig.DefaultLatitude,
                    _settings.LocationConfig.DefaultLongitude).Result;
            }

            set { }
        }

        public bool UsePogoDevHashServer
        {
            get { return _settings.Auth.APIConfig.UsePogoDevAPI; }
            set { _settings.Auth.APIConfig.UsePogoDevAPI = value; }
        }

        public bool UseLegacyAPI
        {
            get { return _settings.Auth.APIConfig.UseLegacyAPI; }
            set { _settings.Auth.APIConfig.UseLegacyAPI = value; }
        }

        public string AuthAPIKey
        {
            get { return _settings.Auth.APIConfig.AuthAPIKey; }
            set { _settings.Auth.APIConfig.AuthAPIKey = value; }
        }

        public bool DisplayVerboseLog
        {
            get { return _settings.Auth.APIConfig.DiplayHashServerLog; }
            set { _settings.Auth.APIConfig.DiplayHashServerLog = value; }
        }
    }
}
