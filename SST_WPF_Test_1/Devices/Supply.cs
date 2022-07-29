namespace SST_WPF_Test_1;

public class Supply : BaseDevice
{
    public double Voltage { get; set; }
    public double Current { get; set; }
    public bool Output { get; set; }
    
    public Supply(string name) : base(name)
    {
        IsDeviceType = $"Блок питания {name}";
    }
}