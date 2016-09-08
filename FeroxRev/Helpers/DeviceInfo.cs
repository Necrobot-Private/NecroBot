using System;

namespace PokemonGo.RocketAPI.Helpers
{
    public class DeviceInfo
    {
        public string AndroidBoardName { get; set; }
        public string AndroidBootloader { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceModelBoot { get; set; }
        public string DeviceModelIdentifier { get; set; }
        public string FirmwareBrand { get; set; }
        public string FirmwareFingerprint { get; set; }
        public string FirmwareTags { get; set; }
        public string FirmwareType { get; set; }
        public string HardwareManufacturer { get; set; }
        public string HardwareModel { get; set; }

        public DeviceInfo()
        {
            if (DeviceId == null)
            {
                if (HardwareManufacturer == "Apple")
                    DeviceId = GenerateRandomDeviceId(20);
                else
                    DeviceId = GenerateRandomDeviceId();
            }
        }

        private static string BytesToHex(byte[] bytes)
        {
            char[] hexArray = "0123456789abcdef".ToCharArray();
            char[] hexChars = new char[bytes.Length * 2];
            for (int index = 0; index < bytes.Length; index++)
            {
                int var = bytes[index] & 0xFF;
                hexChars[index * 2] = hexArray[(int)((uint)var >> 4)];
                hexChars[index * 2 + 1] = hexArray[var & 0x0F];
            }
            return new string(hexChars).ToLower();
        }

        public static string GenerateRandomDeviceId(long numBytes = 16)
        {
            var bytes = new byte[numBytes];
            new Random().NextBytes(bytes);
            return BytesToHex(bytes);
        }
    }
}
