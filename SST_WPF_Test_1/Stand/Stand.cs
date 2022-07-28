using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SST_WPF_Test_1;

public class Stand : Notify
{
    public VoltageCurrentMeter MultimeterStand { get; set; }
    public ObservableCollection<SwitcherMeter> SwitchersMetersStand { get; set; } = new();
    public Thermometer ThermometerStand { get; set; }
    public Supply SupplyStand { get; set; }
    public SmallLoad SmallLoadStand { get; set; }
    public BigLoad BigLoadStand { get; set; }
    public ObservableCollection<Vip> VipsStand { get; set; } = new();

    public TypeOfTestRun testRun;

    /// <summary>
    /// Чей сейчас тест идет
    /// </summary>
    public TypeOfTestRun TestRun
    {
        get => testRun;
        set => Set(ref testRun, value);
    }

    private double percentCurrentTest;

    /// <summary>
    /// На соклько процентов выполнен текущий тест
    /// </summary>
    public double PercentCurrentTest
    {
        get => percentCurrentTest;
        set => Set(ref percentCurrentTest, value);
    }

    public Stand()
    {
        TestRun = TypeOfTestRun.None;
        GetDevices();
    }

    private StatusDeviceTest statusTest;

    /// <summary>
    /// На соклько процентов выполнен текущий тест
    /// </summary>
    public StatusDeviceTest StatusTest
    {
        get => MultimeterStand.StatusTest;
        set
        {
            MultimeterStand.StatusTest = statusTest;
        }
    }

    public void GetDevices()
    {
        if (true) //TODO (true) - если сеарилизатор недосутпен выводим исключение и создаем приборы со станрдартными настройками
        {
            MultimeterStand = new VoltageCurrentMeter("PSW");
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM3", 9600, 1, 0, 8);
            SupplyStand = new Supply("DS");
            SupplyStand.SetConfigDevice(TypePort.SerialInput, "COM4", 2400, 1, 0, 8);
        }
    }

    public async Task<bool> PrimaryCheckDevices()
    {
        TestRun = TypeOfTestRun.None;
        //Уведомляем что начался первычный тест через енум
        TestRun = TypeOfTestRun.PrimaryCheckDevices;

        if (true)
        {
            StatusTest = StatusDeviceTest.Error;
            PercentCurrentTest = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            PercentCurrentTest = 20;
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            PercentCurrentTest = 40;
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            PercentCurrentTest = 60;
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            PercentCurrentTest = 80;
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            MultimeterStand.StatusTest = StatusDeviceTest.Ok;
            PercentCurrentTest = 100;
            //Уведомляем что первичный тест закончен
            TestRun = TypeOfTestRun.PrimaryCheckDevicesReady;
            return true;
        }

        //Уведомляем что первичный тест закончен
        return false;
    }
}

public enum TypeOfTestRun
{
    None,
    PrimaryCheckDevices,
    PrimaryCheckDevicesReady,
    MeasurementZero,
    MeasurementZeroReady,
    WaitSettingToOperatingMode,
    SettingToOperatingModeReady,
    CyclicMeasurement,
    CyclicMeasurementReady,
    Error
}