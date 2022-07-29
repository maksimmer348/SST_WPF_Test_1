using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SST_WPF_Test_1;

public class Stand : Notify
{
    #region Компоноенты стенда

    public VoltageCurrentMeter MultimeterStand { get; set; }
    public ObservableCollection<SwitcherMeter> SwitchersMetersStand { get; set; } = new();

    public ObservableCollection<RelayVip> RelayVipsStand { get; set; } = new();

    public Thermometer ThermometerStand { get; set; }
    public Supply SupplyStand { get; set; }
    public SmallLoad SmallLoadStand { get; set; }
    public BigLoad BigLoadStand { get; set; }

    public Heat HeatStand { get; set; }
    public ObservableCollection<Vip> VipsStand { get; set; } = new();

    #endregion

    #region Статусы стенда

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

    //Временные статусы
    private DateTime testStartTime;

    public DateTime TestStartTime
    {
        get => testStartTime;
        set => Set(ref testStartTime, value);
    }

    private DateTime testEndTime;

    public DateTime TestEndTime
    {
        get => testEndTime;
        set => Set(ref testEndTime, value);
    }

    private DateTime nextMeasurementIn;

    public DateTime NextMeasurementIn
    {
        get => nextMeasurementIn;
        set => Set(ref nextMeasurementIn, value);
    }

    //

    #endregion


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
        set { MultimeterStand.StatusTest = statusTest; }
    }

    public void GetDevices()
    {
        if (true) //TODO (true) - если сеарилизатор недосутпен выводим исключение и создаем приборы со станрдартными настройками
        {
            MultimeterStand = new("VOLT-21") {RowIndex = 0, ColumnIndex = 0};
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM3", 9600, 1, 0, 8);

            SupplyStand = new("SUPPL-787") {RowIndex = 0, ColumnIndex = 1};
            SupplyStand.SetConfigDevice(TypePort.SerialInput, "COM4", 2400, 1, 0, 8);

            ThermometerStand = new("THERM-99") {RowIndex = 0, ColumnIndex = 2};
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM5", 9600, 1, 0, 8);

            SmallLoadStand = new("SMLL LOAD-87") {RowIndex = 0, ColumnIndex = 3};
            SupplyStand.SetConfigDevice(TypePort.SerialInput, "COM6", 2400, 1, 0, 8);

            BigLoadStand = new("BIG LOAD-90") {RowIndex = 0, ColumnIndex = 4};
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM7", 9600, 1, 0, 8);

            HeatStand = new("BIG LOAD-90") {RowIndex = 0, ColumnIndex = 5};
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM19", 9600, 1, 0, 8);


            SwitchersMetersStand = new();

            SwitchersMetersStand.Add(new SwitcherMeter("1") {RowIndex = 1, ColumnIndex = 0});
            SwitchersMetersStand.Add(new SwitcherMeter("2") {RowIndex = 1, ColumnIndex = 1});
            SwitchersMetersStand.Add(new SwitcherMeter("3") {RowIndex = 1, ColumnIndex = 2});
            SwitchersMetersStand.Add(new SwitcherMeter("4") {RowIndex = 1, ColumnIndex = 3});
            SwitchersMetersStand.Add(new SwitcherMeter("5") {RowIndex = 1, ColumnIndex = 4});
            SwitchersMetersStand.Add(new SwitcherMeter("6") {RowIndex = 1, ColumnIndex = 5});
            SwitchersMetersStand.Add(new SwitcherMeter("7") {RowIndex = 2, ColumnIndex = 0});
            SwitchersMetersStand.Add(new SwitcherMeter("8") {RowIndex = 2, ColumnIndex = 1});
            SwitchersMetersStand.Add(new SwitcherMeter("9") {RowIndex = 2, ColumnIndex = 2});
            SwitchersMetersStand.Add(new SwitcherMeter("10") {RowIndex = 2, ColumnIndex = 3});
            SwitchersMetersStand.Add(new SwitcherMeter("11") {RowIndex = 2, ColumnIndex = 4});
            SwitchersMetersStand.Add(new SwitcherMeter("12") {RowIndex = 2, ColumnIndex = 5});
            foreach (var switcherMeter in SwitchersMetersStand)
            {
                switcherMeter.SetConfigDevice(TypePort.SerialInput, "COM8", 9600, 1, 0, 8);
            }


            VipsStand = new();
            VipsStand.Add(new Vip
            {
                Name = "Вип-1", VoltageOut1 = 1, ID = 1, Relay = new RelayVip("1"), RowIndex = 0, ColumnIndex = 0
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-2", VoltageOut1 = 1, ID = 2, Relay = new RelayVip("2"), RowIndex = 0, ColumnIndex = 1
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-3", VoltageOut1 = 1, ID = 3, Relay = new RelayVip("3"), RowIndex = 0, ColumnIndex = 2
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-4", VoltageOut1 = 1, ID = 4, Relay = new RelayVip("4"), RowIndex = 0, ColumnIndex = 3
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-5", VoltageOut1 = 1, ID = 5, Relay = new RelayVip("5"), RowIndex = 1, ColumnIndex = 0
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-6", VoltageOut1 = 1, ID = 6, Relay = new RelayVip("6"), RowIndex = 1, ColumnIndex = 1
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-7", VoltageOut1 = 1, ID = 7, Relay = new RelayVip("7"), RowIndex = 1, ColumnIndex = 2
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-8", VoltageOut1 = 10, ID = 8, Relay = new RelayVip("8"), RowIndex = 1, ColumnIndex = 3
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-9", VoltageOut1 = 22, ID = 9, Relay = new RelayVip("9"), RowIndex = 2, ColumnIndex = 0
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-10", VoltageOut1 = 12, ID = 10, Relay = new RelayVip("10"), RowIndex = 2, ColumnIndex = 1
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-11", VoltageOut1 = 44, ID = 11, Relay = new RelayVip("11"), RowIndex = 2, ColumnIndex = 2
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-12", VoltageOut1 = 44, ID = 12, Relay = new RelayVip("12"), RowIndex = 2, ColumnIndex = 3
            });
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
            foreach (var vip in VipsStand)
            {
                vip.Relay.StatusTest = (StatusDeviceTest) Random.Shared.Next(0, 2);
            }

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