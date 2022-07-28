namespace SST_WPF_Test_1;

public class Thermometer : BaseDevice
{
    public double Temperature { get; set; }

    public Thermometer(string name ) : base(name)
    {
    }
}