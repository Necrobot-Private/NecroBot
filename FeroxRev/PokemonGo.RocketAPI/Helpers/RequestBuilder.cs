using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using System;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        private Crypt crypt;
        private readonly string _authToken;
        private readonly AuthType _authType;
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly double _altitude;
        private readonly AuthTicket _authTicket;
        private int _startTime;
        private ulong _nextRequestId;
        private readonly ISettings settings;

        private ByteString SessionHash
        {
            get { return settings.SessionHash; }
            set { settings.SessionHash = value; }
        }

        public void GenerateNewHash()
        {
            var hashBytes = new byte[16];

            RandomDevice.NextBytes(hashBytes);

            SessionHash = ByteString.CopyFrom(hashBytes);
        }

        public RequestBuilder(string authToken, AuthType authType, double latitude, double longitude, double altitude, ISettings settings, AuthTicket authTicket = null)
        {
            _authToken = authToken;
            _authType = authType;
            _latitude = latitude;
            _longitude = longitude;
            _altitude = altitude;
            this.settings = settings;
            _authTicket = authTicket;
            _nextRequestId = Convert.ToUInt64(RandomDevice.NextDouble() * Math.Pow(10, 18));
            if (_startTime == 0)
                _startTime = Utils.GetTime(true);

            if (SessionHash == null)
            {
                GenerateNewHash();
            }

            if (crypt == null)
                crypt = new Crypt();
        }

        private Unknown6 GenerateSignature(IEnumerable<IMessage> requests)
        {
            var ticketBytes = _authTicket.ToByteArray();

            Signature.Types.DeviceInfo deviceInfo;
            if (settings.HardwareManufacturer.Equals("Apple", StringComparison.Ordinal))
            {
                // iOS
                deviceInfo = new Signature.Types.DeviceInfo()
                {
                    DeviceId = settings.DeviceId,
                    DeviceBrand = settings.DeviceBrand,
                    DeviceModel = settings.DeviceModel,
                    DeviceModelBoot = settings.DeviceModelBoot,
                    HardwareManufacturer = settings.HardwareManufacturer,
                    HardwareModel = settings.HardwareModel,
                    FirmwareBrand = settings.FirmwareBrand,
                    FirmwareType = settings.FirmwareType,
                };
            }
            else
            {
                // Android
                deviceInfo = new Signature.Types.DeviceInfo()
                {
                    DeviceId = settings.DeviceId,
                    AndroidBoardName = settings.AndroidBoardName,
                    AndroidBootloader = settings.AndroidBootloader,
                    DeviceBrand = settings.DeviceBrand,
                    DeviceModel = settings.DeviceModel,
                    DeviceModelIdentifier = settings.DeviceModelIdentifier,
                    DeviceModelBoot = settings.DeviceModelBoot,
                    HardwareManufacturer = settings.HardwareManufacturer,
                    HardwareModel = settings.HardwareModel,
                    FirmwareBrand = settings.FirmwareBrand,
                    FirmwareTags = settings.FirmwareTags,
                    FirmwareType = settings.FirmwareType,
                    FirmwareFingerprint = settings.FirmwareFingerprint
                };
            }

            var sig = new Signature()
            {
				Timestamp = (ulong)Utils.GetTime(true),
                TimestampSinceStart = (ulong)(Utils.GetTime(true) - _startTime),
                LocationHash1 = Utils.GenerateLocation1(ticketBytes, _latitude, _longitude, _altitude),
                LocationHash2 = Utils.GenerateLocation2(_latitude, _longitude, _altitude),
                SensorInfo = new Signature.Types.SensorInfo()
                {
                    AccelNormalizedX = GenRandom(-0.31110161542892456, 0.1681540310382843),
                    AccelNormalizedY = GenRandom(-0.6574847102165222, -0.07290205359458923),
                    AccelNormalizedZ = GenRandom(-0.9943905472755432, -0.7463029026985168),
                    TimestampSnapshot = (ulong)(Utils.GetTime(true) - _startTime - RandomDevice.Next(100, 400)),
                    MagnetometerX = GenRandom(-0.139084026217, 0.138112977147),
                    MagnetometerY = GenRandom(-0.2, 0.19),
                    MagnetometerZ = GenRandom(-0.2, 0.4),
                    AngleNormalizedX = GenRandom(-47.149471283, 61.8397789001),
                    AngleNormalizedY = GenRandom(-47.149471283, 61.8397789001),
                    AngleNormalizedZ = GenRandom(-47.149471283, 5),
                    AccelRawX = GenRandom(0.0729667818829, 0.0729667818829),
                    AccelRawY = GenRandom(-2.788630499244109, 3.0586791383810468),
                    AccelRawZ = GenRandom(-0.34825887123552773, 0.19347580173737935),
                    GyroscopeRawX = GenRandom(-0.9703824520111084, 0.8556089401245117),
                    GyroscopeRawY = GenRandom(-1.7470258474349976, 1.4218578338623047),
                    GyroscopeRawZ = GenRandom(-0.9681901931762695, 0.8396636843681335),
                    AccelerometerAxes = 3
                },
                DeviceInfo = deviceInfo
            };

            sig.LocationFix.Add(new Signature.Types.LocationFix()
            {
                Provider = "fused",
                Latitude = (float)_latitude,
                Longitude = (float)_longitude,
                Altitude = (float)_altitude,
                HorizontalAccuracy = (float)Math.Round(GenRandom(50, 250), 7),
                VerticalAccuracy = RandomDevice.Next(2, 5),
                TimestampSnapshot = (ulong)(Utils.GetTime(true) - _startTime - RandomDevice.Next(100, 300)),
                ProviderStatus = 3,
                LocationType = 1
            });

            foreach (var request in requests)
                sig.RequestHash.Add(Utils.GenerateRequestHash(ticketBytes, request.ToByteArray()));

            sig.SessionHash = SessionHash;
            //sig.Unknown25 = -8537042734809897855; // For 0.33
            sig.Unknown25 = 7363665268261373700; // For 0.35

            Unknown6 val = new Unknown6()
            {
                RequestType = 6,
                Unknown2 = new Unknown6.Types.Unknown2()
                {
                    EncryptedSignature = ByteString.CopyFrom(crypt.Encrypt(sig.ToByteArray()))
                }
            };

            return val;
        }
        
        public RequestEnvelope GetRequestEnvelope(params Request[] customRequests)
        {
            var e = new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = _nextRequestId++, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _latitude, //7
                Longitude = _longitude, //8
                Altitude = _altitude, //9
                AuthTicket = _authTicket, //11
                MsSinceLastLocationfix = RandomDevice.Next(800, 1900) //12
            };
            e.Unknown6.Add(GenerateSignature(customRequests));
            return e;
        }

        public RequestEnvelope GetInitialRequestEnvelope(params Request[] customRequests)
        {
            var e = new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = _nextRequestId++, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _latitude, //7
                Longitude = _longitude, //8
                Altitude = _altitude, //9
                AuthInfo = new RequestEnvelope.Types.AuthInfo
                {
                    Provider = _authType == AuthType.Google ? "google" : "ptc",
                    Token = new RequestEnvelope.Types.AuthInfo.Types.JWT
                    {
                        Contents = _authToken,
                        Unknown2 = 59
                    }
                }, //10
                MsSinceLastLocationfix = RandomDevice.Next(800, 1900) //12
            };
            return e;
        }

        public RequestEnvelope GetRequestEnvelope(RequestType type, IMessage message)
        {
            return GetRequestEnvelope(new Request()
            {
                RequestType = type,
                RequestMessage = message.ToByteString()
            });

        }

        private static readonly Random RandomDevice = new Random();

        public static double GenRandom(double num)
        {
            var randomFactor = 0.3f;
            var randomMin = (num * (1 - randomFactor));
            var randomMax = (num * (1 + randomFactor));
            var randomizedDelay = RandomDevice.NextDouble() * (randomMax - randomMin) + randomMin; ;
            return randomizedDelay; ;
        }

        public static double GenRandom(double min, double max)
        {
            return RandomDevice.NextDouble() * (max - min) + min;
        }
    }
}
