using System;
using System.Globalization;

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

    #region MyRegion

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


    private string enableTypeVipName;

    /// <summary>
    /// Тип Випа
    /// </summary>
    public string EnableTypeVipName
    {
        get => enableTypeVipName;
        set => Set(ref enableTypeVipName, value);
    }

    public double PercentAccuracyVoltages { get; set; }

    public double PercentAccuracyCurrent { get; set; }
    #endregion

    #region Значения для приброров

    public DeviceParameters Parameters = new DeviceParameters();


    public void SetDeviceParameters(DeviceParameters deviceParameters)
    {
        Parameters = deviceParameters;

        //TODO добавить проверки влиадаторы
        //TODO темины сделать через передачу в метод класса DeviceParameters
        // //Parameters.BigLoadValues = new BigLoadValues()


        // {
        //     OutputOn = "1",
        //     OutputOff = "0",
        //     Freq = freq.ToString(CultureInfo.InvariantCulture),
        //     Ampl = ampl.ToString(CultureInfo.InvariantCulture),
        //     Dco = dco.ToString(CultureInfo.InvariantCulture),
        //     Squ = squ.ToString(CultureInfo.InvariantCulture),
        // };
        // Parameters.HeatValues = new HeatValues()
        // {
        //     OutputOn = "1",
        //     OutputOff = "0"
        // };
        // Parameters.SupplyValues = new SupplyValues()
        // {
        //     Voltage = supVoltage.ToString(CultureInfo.InvariantCulture),
        //     Current = supCurrent.ToString(CultureInfo.InvariantCulture)
        // };
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
    public ThermometerValues ThermometerValues { get; set; }
}

public class ThermometerValues : BaseDeviceValues
{

    public ThermometerValues(string outputOn, string outputOff) : base(outputOn, outputOff)
    {
    }
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