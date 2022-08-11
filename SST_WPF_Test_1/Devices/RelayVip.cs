using System;
using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class RelayVip : BaseDevice
{
    public int Id { get; set; }
    [JsonIgnore]
    public bool Output { get; set; }
    [JsonIgnore]
    public RelayVipError ErrorVip { get; set; }
    public RelayVip(string name) : base(name)
    {
        Id = Int32.Parse(name);
        IsDeviceType = $"Реле ВИПА-{Id}";
    }
}

public enum RelayVipError
{
    Error1,
    Error2,
    Error3,
    Error4,
}