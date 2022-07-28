using System;
using System.Windows.Media;

namespace SST_WPF_Test_1;

public class Vip : Notify
{
    public int ID { get; set; }
    public string Name { get; set; }
    public TypeVip Type { get; set; }

    //Текущие значения на Випеs
    public double VoltageOut1 { get; set; }
    public double VoltageOut2 { get; set; }
    public double CurrentIn { get; set; }

    public double Temperature { get; set; }

    public double VoltageIn { get; set; }

    public bool Output { get; set; }

    
    private StatusDeviceTest statusTest;
    public StatusDeviceTest StatusTest
    {
        get => statusTest;
        set => Set(ref statusTest, value, nameof(StatusColor));
    }

    public Brush StatusColor =>
        StatusTest switch
        {
            StatusDeviceTest.Error => Brushes.Red,
            StatusDeviceTest.Ok => Brushes.Green,
            _ => Brushes.Black
        };

    
    private RelayVip Relay;
    
}