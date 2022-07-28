namespace SST_WPF_Test_1;

public class Vip
{
    public string Name { get; set; }
    public TypeVip Type { get; set; }

    //Текущие значения на Випе
    public double VoltageOut1 { get; set; }
    public double VoltageOut2 { get; set; }
    public double CurrentIn { get; set; }

    public double Temperature { get; set; }

    public double VoltageIn { get; set; }

    public bool Output { get; set; }

    public StatusDeviceTest StatusTest { get; set; }
    
    private RelayVip Relay;
}