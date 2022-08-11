using System;
using System.Windows.Media;

namespace SST_WPF_Test_1;

public class Vip : Notify
{
    public int ID { get; set; }
    private string name;

    public string Name
    {
        get => name;
        set => Set(ref name, value);
    }

    public bool IsTested { get; set; }

    public string number;
    public string Number
    {
        get => number;
        set
        {
            if (!Set(ref number, value, nameof(Name)))return;//, nameof(IsTested))) return;

            if (!string.IsNullOrWhiteSpace(number))
            {
                Name = $"Вип-{ID}, Номер-{number}";
                //IsTested = true;
            }
            else
            {
                Name = $"Вип-{ID}";
                //IsTested = false;
            }
        }
    }
    
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
            _ => Brushes.DarkGray
        };
    
    
    public TypeVip Type { get; set; }

    //Текущие значения на Випе
    public double VoltageOut1 { get; set; }
    public double VoltageOut2 { get; set; }
    public double CurrentIn { get; set; }
    public double Temperature { get; set; }
    public double VoltageIn { get; set; }
    public bool Output { get; set; }
    
    public RelayVip Relay { get; set; }
    
    //
    public int RowIndex { get; set; }

    public int ColumnIndex { get; set; }
    //

  

}