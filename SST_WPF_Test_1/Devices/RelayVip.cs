namespace SST_WPF_Test_1;

public class RelayVip : BaseDevice
{
    public bool Output { get; set; }
    public RelayVipError ErrorVip { get; set; }
    public RelayVip(string name) : base(name)
    {
        
    }
}

public enum RelayVipError
{
    Error1,
    Error2,
    Error3,
    Error4,
}