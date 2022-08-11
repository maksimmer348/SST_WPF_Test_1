using System;
using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class SwitcherMeter : BaseDevice
{
    public int Id { get; set; }
    [JsonIgnore]
    public bool Output { get; set; }
    
    public SwitcherMeter(string name) : base(name)
    {
        Id = Int32.Parse(name);
        IsDeviceType = $"Переключатель-{name}";
    }
}