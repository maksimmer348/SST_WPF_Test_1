using System;
using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class RelayVip : BaseDevice
{
    public int Id { get; set; }

    /// <summary>
    /// Статус выхода устройства
    /// </summary>
    [JsonIgnore]
    public bool Output { get; set; }

    /// <summary>
    /// Вид ошибки Випа
    /// </summary>
    [JsonIgnore]
    public RelayVipError ErrorVip { get; set; }

    public RelayVip(string name) : base(name)
    {
        Id = Int32.Parse(name);
        IsDeviceType = $"Реле ВИПА-{Id}";
    }
}