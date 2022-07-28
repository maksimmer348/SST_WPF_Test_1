namespace SST_WPF_Test_1;

public class ConfigDeviceParams
{
    public TypePort TypePort{ get; set; }
    public string PortName{ get; set; }
    public int Baud{ get; set; }
    public int StopBits{ get; set; }
    public int Parity{ get; set; }
    public int DataBits{ get; set; }
    public bool Dtr { get; set; }
}