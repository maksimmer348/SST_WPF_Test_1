namespace SST_WPF_Test_1;

public class SwitcherMeter : BaseDevice
{
    public int ID { get; set; }
    public bool Output { get; set; }
    
    public SwitcherMeter(string name) : base(name)
    {
        IsDeviceType = $"Переключатель № {name}";
    }
}