using PokemonGo.RocketAPI.Helpers;
using System;
using System.Collections.Generic;

namespace PoGo.NecroBot.Logic.Utils
{
    public static class DeviceInfoHelper
    {
        private static string[][] IosDeviceInfo = new string[][]
            {
                new string[] {"iPad3,1", "iPad", "J1AP"},
                new string[] {"iPad3,2", "iPad", "J2AP"},
                new string[] {"iPad3,3", "iPad", "J2AAP"},
                new string[] {"iPad3,4", "iPad", "P101AP"},
                new string[] {"iPad3,5", "iPad", "P102AP"},
                new string[] {"iPad3,6", "iPad", "P103AP"},

                new string[] {"iPad4,1", "iPad", "J71AP"},
                new string[] {"iPad4,2", "iPad", "J72AP"},
                new string[] {"iPad4,3", "iPad", "J73AP"},
                new string[] {"iPad4,4", "iPad", "J85AP"},
                new string[] {"iPad4,5", "iPad", "J86AP"},
                new string[] {"iPad4,6", "iPad", "J87AP"},
                new string[] {"iPad4,7", "iPad", "J85mAP"},
                new string[] {"iPad4,8", "iPad", "J86mAP"},
                new string[] {"iPad4,9", "iPad", "J87mAP"},

                new string[] {"iPad5,1", "iPad", "J96AP"},
                new string[] {"iPad5,2", "iPad", "J97AP"},
                new string[] {"iPad5,3", "iPad", "J81AP"},
                new string[] {"iPad5,4", "iPad", "J82AP"},

                new string[] {"iPad6,7", "iPad", "J98aAP"},
                new string[] {"iPad6,8", "iPad", "J99aAP"},

                new string[] {"iPhone5,1", "iPhone", "N41AP"},
                new string[] {"iPhone5,2", "iPhone", "N42AP"},
                new string[] {"iPhone5,3", "iPhone", "N48AP"},
                new string[] {"iPhone5,4", "iPhone", "N49AP"},

                new string[] {"iPhone6,1", "iPhone", "N51AP"},
                new string[] {"iPhone6,2", "iPhone", "N53AP"},

                new string[] {"iPhone7,1", "iPhone", "N56AP"},
                new string[] {"iPhone7,2", "iPhone", "N61AP"},

                new string[] {"iPhone8,1", "iPhone", "N71AP"}
            };

        private static string[] IosVersions = { "8.1.1", "8.1.2", "8.1.3", "8.2", "8.3", "8.4", "8.4.1", "9.0", "9.0.1", "9.0.2", "9.1", "9.2", "9.2.1", "9.3", "9.3.1", "9.3.2", "9.3.3", "9.3.4" };
        
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

        public static DeviceInfo GetRandomIosDevice()
        {
            DeviceInfo deviceInfo = new DeviceInfo();

            // iOS device id (UDID) are 20 bytes long.
            var bytes = new byte[20];
            new Random().NextBytes(bytes);
            var deviceId = BytesToHex(bytes);

            deviceInfo.DeviceId = deviceId;
            deviceInfo.FirmwareType = IosVersions[new Random().Next(IosVersions.Length)];
            string[] device = IosDeviceInfo[(new Random()).Next(IosDeviceInfo.Length)];
            deviceInfo.DeviceModelBoot = device[0];
            deviceInfo.DeviceModel = device[1];

            deviceInfo.HardwareModel = device[2];
            deviceInfo.FirmwareBrand = "iPhone OS";
            deviceInfo.DeviceBrand = "Apple";
            deviceInfo.HardwareManufacturer = "Apple";

            return deviceInfo;
        }

        public static Dictionary<string, DeviceInfo> AndroidDeviceInfoSets = new Dictionary<string, DeviceInfo>() {
            { "lg-optimus-g",
                new DeviceInfo()
                {
                    AndroidBoardName = "geehrc",
                    AndroidBootloader = "MAKOZ10f",
                    DeviceBrand = "LGE",
                    DeviceModel = "LG-LS970",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "cm_ls970",
                    FirmwareBrand = "cm_ls970",
                    FirmwareFingerprint = "google/occam/mako:4.2.2/JDQ39/573038:user/release-keys",
                    FirmwareTags = "test-keys",
                    FirmwareType = "userdebug",
                    HardwareManufacturer = "LGE",
                    HardwareModel = "LG-LS970"
                }
            },
            { "nexus7gen2",
                new DeviceInfo()
                {
                    AndroidBoardName = "flo",
                    AndroidBootloader = "FLO-04.07",
                    DeviceBrand = "google",
                    DeviceModel = "Nexus 7",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "razor",
                    FirmwareBrand = "razor",
                    FirmwareFingerprint = "google/razor/flo:6.0.1/MOB30P/2960889:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "asus",
                    HardwareModel = "Nexus 7"
                }
            },
            { "nexus7gen1",
                new DeviceInfo()
                {
                    AndroidBoardName = "grouper",
                    AndroidBootloader = "4.23",
                    DeviceBrand = "google",
                    DeviceModel = "Nexus 7",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "nakasi",
                    FirmwareBrand = "nakasi",
                    FirmwareFingerprint = "google/nakasi/grouper:5.1.1/LMY47V/1836172:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "asus",
                    HardwareModel = "Nexus 7"
                }
            },
            { "htc10",
                new DeviceInfo()
                {
                    AndroidBoardName = "msm8996",
                    AndroidBootloader = "1.0.0.0000",
                    DeviceBrand = "HTC",
                    DeviceModel = "HTC 10",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "pmewl_00531",
                    FirmwareBrand = "pmewl_00531",
                    FirmwareFingerprint = "htc/pmewl_00531/htc_pmewl:6.0.1/MMB29M/770927.1:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "HTC",
                    HardwareModel = "HTC 10"
                }
            },
            { "galaxy6",
                new DeviceInfo()
                {
                    AndroidBoardName = "universal7420",
                    AndroidBootloader = "G920FXXU3DPEK",
                    DeviceBrand = "samsung",
                    DeviceModel = "zeroflte",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "SM-G920F",
                    FirmwareBrand = "zerofltexx",
                    FirmwareFingerprint = "samsung/zerofltexx/zeroflte:6.0.1/MMB29K/G920FXXU3DPEK:user/release-keys",
                    FirmwareTags = "dev-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "samsungexynos7420"
                }
            },
            { "galaxy-s5-gold",
                new DeviceInfo()
                {
                    AndroidBoardName = "MSM8974",
                    AndroidBootloader = "G900FXXU1CPEH",
                    DeviceBrand = "samsung",
                    DeviceModel = "SM-G900F",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "kltexx",
                    FirmwareBrand = "kltexx",
                    FirmwareFingerprint = "samsung/kltexx/klte:6.0.1/MMB29M/G900FXXU1CPEH:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SM-G900F"
                }
            },
            { "lg-optimus-f6",
                new DeviceInfo()
                {
                    AndroidBoardName = "f6t",
                    AndroidBootloader = "1.0.0.0000",
                    DeviceBrand = "lge",
                    DeviceModel = "LG-D500",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "f6_tmo_us",
                    FirmwareBrand = "f6_tmo_us",
                    FirmwareFingerprint = "lge/f6_tmo_us/f6:4.1.2/JZO54K/D50010h.1384764249:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "LGE",
                    HardwareModel = "LG-D500"
                }
            },
            { "nexus-5x",
                new DeviceInfo()
                {
                    AndroidBoardName = "bullhead",
                    AndroidBootloader = "BHZ10k",
                    DeviceBrand = "google",
                    DeviceModel = "Nexus 5X",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "bullhead",
                    FirmwareBrand = "bullhead",
                    FirmwareFingerprint = "google/bullhead/bullhead:6.0.1/MTC19T/2741993:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "LGE",
                    HardwareModel = "Nexus 5X"
                }
            },
            { "galaxy-s7-edge",
                new DeviceInfo()
                {
                    AndroidBoardName = "msm8996",
                    AndroidBootloader = "G935TUVU3APG1",
                    DeviceBrand = "samsung",
                    DeviceModel = "SM-G935T",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "hero2qltetmo",
                    FirmwareBrand = "hero2qltetmo",
                    FirmwareFingerprint = "samsung/hero2qltetmo/hero2qltetmo:6.0.1/MMB29M/G935TUVU3APG1:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SM-G935T"
                }
            },
            { "xperia-z5",
                new DeviceInfo()
                {
                    AndroidBoardName = "msm8994",
                    AndroidBootloader = "s1",
                    DeviceBrand = "Sony",
                    DeviceModel = "E6653",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "E6653",
                    FirmwareBrand = "E6653",
                    FirmwareFingerprint = "Sony/E6653/E6653:6.0.1/32.2.A.0.224/456768306:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "Sony",
                    HardwareModel = "E6653"
                }
            },
            { "galaxy-s4",
                new DeviceInfo()
                {
                    AndroidBoardName = "MSM8960",
                    AndroidBootloader = "I337MVLUGOH1",
                    DeviceBrand = "samsung",
                    DeviceModel = "SGH-I337M",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "jfltevl",
                    FirmwareBrand = "jfltevl",
                    FirmwareFingerprint = "samsung/jfltevl/jfltecan:5.0.1/LRX22C/I337MVLUGOH1:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SGH-I337M"
                }
            },
            { "nexus-6p",
                new DeviceInfo()
                {
                    AndroidBoardName = "angler",
                    AndroidBootloader = "angler-03.52",
                    DeviceBrand = "google",
                    DeviceModel = "Nexus 6P",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "angler",
                    FirmwareBrand = "angler",
                    FirmwareFingerprint = "google/angler/angler:6.0.1/MTC19X/2960136:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "Huawei",
                    HardwareModel = "Nexus 6P"
                }
            },
            { "sony-z3-compact",
                new DeviceInfo()
                {
                    AndroidBoardName = "MSM8974",
                    AndroidBootloader = "s1",
                    DeviceBrand = "docomo",
                    DeviceModel = "SO-02G",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "SO-02G",
                    FirmwareBrand = "SO-02G",
                    FirmwareFingerprint = "docomo/SO-02G/SO-02G:5.0.2/23.1.B.1.317/2161656255:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "Sony",
                    HardwareModel = "SO-02G"
                }
            },
            { "galaxy-tab3",
                new DeviceInfo()
                {
                    AndroidBoardName = "smdk4x12",
                    AndroidBootloader = "T310UEUCOI1",
                    DeviceBrand = "samsung",
                    DeviceId = "8525f5d8201f78b5",
                    DeviceModel = "SM-T310",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "lt01wifiue",
                    FirmwareBrand = "lt01wifiue",
                    FirmwareFingerprint = "samsung/lt01wifiue/lt01wifi:4.4.2/KOT49H/T310UEUCOI1:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SM-T310"
                }
            },
            { "nexus5",
                new DeviceInfo()
                {
                    AndroidBoardName = "hammerhead",
                    AndroidBootloader = "HHZ20b",
                    DeviceBrand = "google",
                    DeviceId = "8525f5d8201f78b5",
                    DeviceModel = "Nexus 5",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "hammerhead",
                    FirmwareBrand = "hammerhead",
                    FirmwareFingerprint = "google/hammerhead/hammerhead:6.0.1/MOB30M/2862625:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "LGE",
                    HardwareModel = "Nexus 5"
                }
            },
            { "galaxy-note-edge",
                new DeviceInfo()
                {
                    AndroidBoardName = "APQ8084",
                    AndroidBootloader = "N915W8VLU1CPE2",
                    DeviceBrand = "samsung",
                    DeviceModel = "SM-N915W8",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "tbltecan",
                    FirmwareBrand = "tbltecan",
                    FirmwareFingerprint = "samsung/tbltecan/tbltecan:6.0.1/MMB29M/N915W8VLU1CPE2:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SM-N915W8"
                }
            },
            { "nexus4-chroma",
                new DeviceInfo()
                {
                    AndroidBoardName = "MAKO",
                    AndroidBootloader = "MAKOZ30f",
                    DeviceBrand = "google",
                    DeviceModel = "Nexus 4",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "occam",
                    FirmwareBrand = "occam",
                    FirmwareFingerprint = "google/occam/mako:6.0.1/MOB30Y/3067468:user/release-keys",
                    FirmwareTags = "test-keys",
                    FirmwareType = "userdebug",
                    HardwareManufacturer = "LGE",
                    HardwareModel = "Nexus 4"
                }
            },
            { "yureka",
                new DeviceInfo()
                {
                    AndroidBoardName = "MSM8916",
                    AndroidBootloader = "tomato-12-gf7e8024",
                    DeviceBrand = "YU",
                    DeviceModel = "AO5510",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "YUREKA",
                    FirmwareBrand = "YUREKA",
                    FirmwareFingerprint = "YU/YUREKA/YUREKA:5.0.2/LRX22G/YNG1TAS1K0:user/release-keys",
                    FirmwareTags = "test-keys",
                    FirmwareType = "userdebug",
                    HardwareManufacturer = "YU",
                    HardwareModel = "AO5510"
                }
            },
            { "note3",
                new DeviceInfo()
                {
                    AndroidBoardName = "MSM8974",
                    AndroidBootloader = "N900PVPUEOK2",
                    DeviceBrand = "samsung",
                    DeviceModel = "SM-N900P",
                    DeviceModelBoot = "qcom",
                    DeviceModelIdentifier = "cm_hltespr",
                    FirmwareBrand = "cm_hltespr",
                    FirmwareFingerprint = "samsung/hltespr/hltespr:5.0/LRX21V/N900PVPUEOH1:user/release-keys",
                    FirmwareTags = "test-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SM-N900P"
                }
            },
            { "galaxy-tab-s84",
                new DeviceInfo()
                {
                    AndroidBoardName = "universal5420",
                    AndroidBootloader = "T705XXU1BOL2",
                    DeviceBrand = "samsung",
                    DeviceModel = "Samsung Galaxy Tab S 8.4 LTE",
                    DeviceModelBoot = "universal5420",
                    DeviceModelIdentifier = "LRX22G.T705XXU1BOL2",
                    FirmwareBrand = "Samsung Galaxy Tab S 8.4 LTE",
                    FirmwareFingerprint = "samsung/klimtltexx/klimtlte:5.0.2/LRX22G/T705XXU1BOL2:user/release-keys",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user",
                    HardwareManufacturer = "samsung",
                    HardwareModel = "SM-T705"
                }
            },

        };
    }
}
