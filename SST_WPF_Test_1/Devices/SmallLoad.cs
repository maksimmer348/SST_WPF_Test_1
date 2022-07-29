namespace SST_WPF_Test_1;

public class SmallLoad : BaseDevice
{
    public SmallLoad(string name) : base(name)
    {
        IsDeviceType = $"Малая нагрузка - {name}";
    }
}