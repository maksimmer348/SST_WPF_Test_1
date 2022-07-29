namespace SST_WPF_Test_1;

public class Heat : BaseDevice
{
    public double Temperature { get; set; }
    public bool Output { get; set; }
    public Heat(string name ) : base(name)
    {
        IsDeviceType = $"Нагрев {name}";
    }
}