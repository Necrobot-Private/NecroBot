using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class DataSharingConfig : BaseConfig
    {
        public DataSharingConfig() : base()
        {
        }

        [NecrobotConfig(Description = "ALlow bot send pokemon data to share data serice", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool EnableSyncData
        {
            get; set;
        }

        //may need add support for web services/wcf/resful later. for now we use most modern web socket things.
        [NecrobotConfig(Description = "Data service enpoint ", Position = 2)]
        [DefaultValue("ws://necrosocket.herokuapp.com/socket.io/?EIO=3&transport=websocket")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string DataRecieverURL { get; set; }

        [NecrobotConfig(Description = "Allow bot auto snipe pokemon whenever has feed send back from server", Position = 3)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool AutoSnipe { get; set; }

        [NecrobotConfig(
             Description =
                 "A unique ID you make by yourself to do a manual snipe from mypogosnipers.com. You have to make sure it is unique",
             Position = 4)]
        [DefaultValue("")]
        [MaxLength(256)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]

        public string DataServiceIdentification { get; set; }

        [NecrobotConfig(Description = "The authozied access key to use snipe data", Position = 4)]
        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string SnipeDataAccessKey { get;  set; }

        [NecrobotConfig(Description = "Enable failover data servers", Position = 5)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool EnableFailoverDataServers { get; set; }

        [NecrobotConfig(Description = "List of servers that bot will connect when primary server down or can't connect", Position = 6)]
        [DefaultValue("ws://s1.mypogosnipers.com/socket.io/?EIO=3&transport=websocket;ws://s2.mypogosnipers.com/socket.io/?EIO=3&transport=websocket")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string FailoverDataServers { get; set; }

    }
}