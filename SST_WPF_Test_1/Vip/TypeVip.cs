namespace SST_WPF_Test_1;

public class TypeVip
{
    public string Type { get; set; }

    //максимальные значения во время цикла испытаниий 1...n, они означают ошибку
    
    public double MaxTemperature { get; set; }
    public double MaxVoltageIn { get; set; }

    private double maxVoltageOut1;
    public double MaxVoltageOut1
    {
        get => maxVoltageOut1;
        set
        {
            maxVoltageOut1 = value;
            PrepareMaxVoltageOut1 = value;
        }
    }

    private double maxVoltageOut2;
    public double MaxVoltageOut2
    {
        get => maxVoltageOut2;
        set
        {
            maxVoltageOut2 = value;
            PrepareMaxVoltageOut2 = value;
        }
    }

    public double MaxCurrentIn { get; set; }
    
    //максимальные значения во время замера 0
    public double PrepareMaxCurrentIn { get; set; }
    public double PrepareMaxVoltageOut1 { get; set; }
    public double PrepareMaxVoltageOut2 { get; set; }
}