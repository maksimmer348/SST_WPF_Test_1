namespace SST_WPF_Test_1;

public class  VoltageCurrentMeter : BaseDevice
{
    
    public double VoltageOut1 { get; set; }
    public double VoltageOut2 { get; set; }

    MeterMode Mode { get; set; }

    public VoltageCurrentMeter(string name) : base(name)
    {
        IsDeviceType = "Вольтметр";
    }
}

internal enum MeterMode
{
    VoltageOut1,
    VoltageOut2
}