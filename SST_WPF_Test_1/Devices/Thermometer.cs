using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class Thermometer : BaseDevice
{
    [JsonIgnore]
    public double Temperature { get; set; }

    public Thermometer(string name ) : base(name)
    {
        IsDeviceType = "Термометр";
    }
}