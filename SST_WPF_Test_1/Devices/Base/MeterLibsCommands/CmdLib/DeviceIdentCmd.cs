using Newtonsoft.Json;

namespace SST_WPF_Test_1;
    public class DeviceIdentCmd
    {
        //public TypeDevice TypeDevice { get; set; }
        
        /// <summary>
        /// Имя устройства
        /// </summary>
        [JsonProperty("nameDevice")]
        public string NameDevice { get; set; }
        /// <summary>
        /// Имя команды
        /// </summary>
        [JsonProperty("nameCmd")]
        public string NameCmd { get; set; }
    }
