namespace SST_WPF_Test_1;

public class BigLoad : BaseDevice
{
    public BigLoad(string name) : base(name)
    {
        IsDeviceType = $"Большая нагрузка/Генератор - {name}";
    }
}