using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class TypeVip : Notify
{
    private string type;

    /// <summary>
    /// Тип Випа
    /// </summary>
    public string Type
    {
        get => type;
        set => Set(ref type, value);
    }

    #region Значения для Випов

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

    private bool enableTypeVipName = true;

    public bool EnableTypeVipName
    {
        get => enableTypeVipName;
        set => Set(ref enableTypeVipName, value);
    }

    public double PercentAccuracyVoltages { get; set; }

    public double PercentAccuracyCurrent { get; set; }

    #endregion

    //

    #region Значения для приброров

    public ObservableCollection<BaseDeviceValues> BaseDeviceValues = new ObservableCollection<BaseDeviceValues>();

    public DeviceParameters Parameters;

    public void SetDeviceParameters(DeviceParameters dp)
    {
        Parameters = dp;
    }

    public DeviceParameters GetDeviceParameters()
    {
        try
        {
            return Parameters;
        }
        catch (Exception e)
        {
            throw new VipException("VipException: Параметры випа не заданы");
        }
    }

    #endregion
}

public class DeviceParameters
{
    public BigLoadValues BigLoadValues { get; set; }
    public HeatValues HeatValues { get; set; }
    public SupplyValues SupplyValues { get; set; }
    public ThermoVoltmeterValues ThermoVoltmeterValues { get; set; }
}


public class ThermoVoltmeterValues : BaseDeviceValues
{
    [JsonIgnore] public string ReturnFuncGDM { get; set; }
    [JsonIgnore] public string ReturnVoltageGDM { get; set; }
    
    private ModeThermoVoltmeter mode;

    [JsonIgnore]
    public ModeThermoVoltmeter Mode
    {
        get
        {
            return mode;
        }
        set
        {
            SetFuncVoltageGDM();
            mode = value;
        }
    }

    //
    
    private string voltageMaxLimit;

    public string VoltageMaxLimit
    {
        get
        {
            return voltageMaxLimit;
        }
        set
        {
            SetFuncVoltageGDM();
            voltageMaxLimit = value;
        }
    }

    public ThermoVoltmeterValues(string voltageMaxLimit, string outputOn, string outputOff) : base(outputOn, outputOff)
    {
        VoltageMaxLimit = voltageMaxLimit;
        SetFuncVoltageGDM();
    }

    void SetFuncVoltageGDM()
    {
        if (VoltageMaxLimit == null)
        {
           return;
        }
        else if (double.Parse(VoltageMaxLimit) == 0.1)
        {
            ReturnVoltageGDM = "1";
        }
        else if (int.Parse(VoltageMaxLimit) == 1)
        {
            ReturnVoltageGDM = "2";
        }
        else if (int.Parse(VoltageMaxLimit) == 10)
        {
            ReturnVoltageGDM = "3";
        }
        else if (int.Parse(VoltageMaxLimit) == 100)
        {
            ReturnVoltageGDM = "4";
        }
        else if (int.Parse(VoltageMaxLimit) == 1000)
        {
            ReturnVoltageGDM = "5";
        }

        if (Mode == ModeThermoVoltmeter.Voltage)
        {
            ReturnFuncGDM = "1";
        }

        if (Mode == ModeThermoVoltmeter.Themperature)
        {
            ReturnFuncGDM = "9";
        }
    }
}

public enum ModeThermoVoltmeter
{
    None,
    Voltage,
    Themperature
}

public class BaseDeviceValues
{
    public string OutputOn { get; set; }
    public string OutputOff { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputOn">Включить выход</param>
    /// <param name="outputOff">Выключить выход</param>
    public BaseDeviceValues(string outputOn, string outputOff)
    {
        OutputOn = outputOn;
        OutputOff = outputOff;
    }
}

public class BigLoadValues : BaseDeviceValues
{
    public string Freq { get; set; }
    public string Ampl { get; set; }
    public string Dco { get; set; }
    public string Squ { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="freq">Частота</param>
    /// <param name="ampl">Амплитуда</param>
    /// <param name="dco">Л=DCO</param>
    /// <param name="squ">SQU</param>
    /// <param name="outputOn">Вкл</param>
    /// <param name="outputOff">Выкл</param>
    public BigLoadValues(string freq, string ampl, string dco, string squ, string outputOn, string outputOff) : base(
        outputOn, outputOff)
    {
        Freq = freq;
        Ampl = ampl;
        Dco = dco;
        Squ = squ;
    }
}

public class HeatValues : BaseDeviceValues
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputOn">Вкл</param>
    /// <param name="outputOff">Выкл</param>
    public HeatValues(string outputOn, string outputOff) : base(outputOn, outputOff)
    {
        //тут ничего не будет не отвелкайся
    }
}

public class SupplyValues : BaseDeviceValues
{
    public string Voltage { get; set; }
    public string Current { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="voltage">Напряжение</param>
    /// <param name="current">Ток</param>
    public SupplyValues(string voltage, string current, string outputOn, string outputOff) : base(outputOn, outputOff)
    {
        Voltage = voltage;
        Current = current;
    }
}
