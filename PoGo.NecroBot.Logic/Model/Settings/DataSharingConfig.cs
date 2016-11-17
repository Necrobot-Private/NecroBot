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
    public class DataSharingConfig
    {
        [ExcelConfig (Description ="ALlow bot send pokemon data to share data serice", Position =1)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool EnableSyncData = true;

        //may need add support for web services/wcf/resful later. for now we use most modern web socket things.
        [ExcelConfig(Description = "Data service enpoint ", Position = 2)]
        [DefaultValue("ws://necrosocket.herokuapp.com/socket.io/?EIO=3&transport=websocket")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 2)]
        public string DataRecieverURL = "ws://necrosocket.herokuapp.com/socket.io/?EIO=3&transport=websocket";

        [ExcelConfig(Description = "Allow bot auto snipe pokemon whenever has feed send back from server", Position = 3)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        public bool AutoSnipe = false;
    }
}
