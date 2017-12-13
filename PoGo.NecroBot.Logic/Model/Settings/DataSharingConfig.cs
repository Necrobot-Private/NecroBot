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

        [NecroBotConfig(Description = "ALlow bot send pokemon data to share data serice", Position = 1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool EnableSyncData
        {
            get; set;
        }

        //may need add support for web services/wcf/resful later. for now we use most modern web socket things.
        [NecroBotConfig(Description = "Data Service Endpoint", Position = 2)]
        [DefaultValue("ws://www.mypogosnipers.com/socket.io/?EIO=3&transport=websocket")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string DataRecieverURL { get; set; }

        [NecroBotConfig(Description = "Allows bot to auto snipe pokemon whenever it has feed sent back from server", Position = 3)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool AutoSnipe { get; set; }

        [NecroBotConfig(Description = "A unique ID you make by yourself to do a manual snipe from mypogosnipers.com. You have to make sure it is unique", Position = 4)]
        [DefaultValue("")]
        [MaxLength(256)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]

        public string DataServiceIdentification { get; set; }

        [NecroBotConfig(Description = "The authorized access key to use snipe data", Position = 4)]
        [DefaultValue("")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string SnipeDataAccessKey { get;  set; }

        [NecroBotConfig(Description = "Enable failover data servers", Position = 5)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool EnableFailoverDataServers { get; set; }

        [NecroBotConfig(Description = "List of servers that bot will connect to when primary server is down or can't be connected to", Position = 6)]
        [DefaultValue("ws://s1.mypogosnipers.com/socket.io/?EIO=3&transport=websocket;ws://s2.mypogosnipers.com/socket.io/?EIO=3&transport=websocket;ws://necrosocket.herokuapp.com/socket.io/?EIO=3&transport=websocket")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public string FailoverDataServers { get; set; }

    }
}
