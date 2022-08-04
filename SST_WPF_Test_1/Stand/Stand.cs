using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace SST_WPF_Test_1;

public class Stand : Notify
{
    #region Компоноенты стенда

    public VoltageCurrentMeter MultimeterStand { get; set; }
    public ObservableCollection<SwitcherMeter> SwitchersMetersStand { get; set; } = new();
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

    public BaseDevice testCurrentDevice;

    /// <summary>
    /// Чей сейчас тест идет
    /// </summary>
    public BaseDevice TestCurrentDevice
    {
        get => testCurrentDevice;
        set => Set(ref testCurrentDevice, value);
    }


    private const double MAX_PERCENT = 100;
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

    #region Время испытаний

    private TimeSpan testAllTime;

    public TimeSpan TestAllTime
    {
        get => testAllTime;
        set => Set(ref testAllTime, value);
    }

    private TimeSpan testIntervalTime;

    public TimeSpan TestIntervalTime
    {
        get => testIntervalTime;
        set => Set(ref testIntervalTime, value);
    }

    private TimeSpan testLeftEndTime;

    public TimeSpan TestLeftEndTime
    {
        get => testLeftEndTime;
        set => Set(ref testLeftEndTime, value);
    }

    #endregion

    #region Отчет

  
    private string fileName;

    public string FileName
    {
        get => fileName;
        set => Set(ref fileName, value);
    }


    #endregion

    public Stand()
    {
        SetDevices();
    }

    public void SetDevices()
    {
        if (true) //TODO (true) - если сеарилизатор недосутпен выводим исключение и создаем приборы со станрдартными настройками
        {
            MultimeterStand = new("GDM-78255A") { RowIndex = 0, ColumnIndex = 0 };
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM3", 9600, 1, 0, 8);

            SupplyStand = new("PSW7-800-2.88") { RowIndex = 0, ColumnIndex = 1 };
            SupplyStand.SetConfigDevice(TypePort.SerialInput, "COM4", 2400, 1, 0, 8);

            ThermometerStand = new("THERM-99") { RowIndex = 0, ColumnIndex = 2 };
            ThermometerStand.SetConfigDevice(TypePort.SerialInput, "COM5", 9600, 1, 0, 8);

            SmallLoadStand = new("SMLL LOAD-87") { RowIndex = 0, ColumnIndex = 3 };
            SmallLoadStand.SetConfigDevice(TypePort.SerialInput, "COM6", 2400, 1, 0, 8);

            BigLoadStand = new("BIG LOAD-90") { RowIndex = 0, ColumnIndex = 4 };
            BigLoadStand.SetConfigDevice(TypePort.SerialInput, "COM7", 9600, 1, 0, 8);

            HeatStand = new("Heat") { RowIndex = 0, ColumnIndex = 5 };
            HeatStand.SetConfigDevice(TypePort.SerialInput, "COM8", 9600, 1, 0, 8);


            SwitchersMetersStand = new();
            SwitchersMetersStand.Add(new SwitcherMeter("1") { RowIndex = 1, ColumnIndex = 0 });
            SwitchersMetersStand.Add(new SwitcherMeter("2") { RowIndex = 1, ColumnIndex = 1 });
            SwitchersMetersStand.Add(new SwitcherMeter("3") { RowIndex = 1, ColumnIndex = 2 });
            SwitchersMetersStand.Add(new SwitcherMeter("4") { RowIndex = 1, ColumnIndex = 3 });
            SwitchersMetersStand.Add(new SwitcherMeter("5") { RowIndex = 1, ColumnIndex = 4 });
            SwitchersMetersStand.Add(new SwitcherMeter("6") { RowIndex = 1, ColumnIndex = 5 });
            SwitchersMetersStand.Add(new SwitcherMeter("7") { RowIndex = 2, ColumnIndex = 0 });
            SwitchersMetersStand.Add(new SwitcherMeter("8") { RowIndex = 2, ColumnIndex = 1 });
            SwitchersMetersStand.Add(new SwitcherMeter("9") { RowIndex = 2, ColumnIndex = 2 });
            SwitchersMetersStand.Add(new SwitcherMeter("10") { RowIndex = 2, ColumnIndex = 3 });
            SwitchersMetersStand.Add(new SwitcherMeter("11") { RowIndex = 2, ColumnIndex = 4 });
            SwitchersMetersStand.Add(new SwitcherMeter("12") { RowIndex = 2, ColumnIndex = 5 });


            foreach (var switcherMeter in SwitchersMetersStand)
            {
                switcherMeter.SetConfigDevice(TypePort.SerialInput, "COM9", 9600, 1, 0, 8);
            }

            VipsStand = new();
            VipsStand.Add(new Vip
            {
                Name = "Вип-1",
                VoltageOut1 = 1,
                ID = 1,
                Relay = new RelayVip("1"),
                RowIndex = 0,
                ColumnIndex = 0
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-2",
                VoltageOut1 = 1,
                ID = 2,
                Relay = new RelayVip("2"),
                RowIndex = 0,
                ColumnIndex = 1
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-3",
                VoltageOut1 = 1,
                ID = 3,
                Relay = new RelayVip("3"),
                RowIndex = 0,
                ColumnIndex = 2
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-4",
                VoltageOut1 = 1,
                ID = 4,
                Relay = new RelayVip("4"),
                RowIndex = 0,
                ColumnIndex = 3
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-5",
                VoltageOut1 = 1,
                ID = 5,
                Relay = new RelayVip("5"),
                RowIndex = 1,
                ColumnIndex = 0
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-6",
                VoltageOut1 = 1,
                ID = 6,
                Relay = new RelayVip("6"),
                RowIndex = 1,
                ColumnIndex = 1
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-7",
                VoltageOut1 = 1,
                ID = 7,
                Relay = new RelayVip("7"),
                RowIndex = 1,
                ColumnIndex = 2
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-8",
                VoltageOut1 = 10,
                ID = 8,
                Relay = new RelayVip("8"),
                RowIndex = 1,
                ColumnIndex = 3
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-9",
                VoltageOut1 = 22,
                ID = 9,
                Relay = new RelayVip("9"),
                RowIndex = 2,
                ColumnIndex = 0
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-10",
                VoltageOut1 = 12,
                ID = 10,
                Relay = new RelayVip("10"),
                RowIndex = 2,
                ColumnIndex = 1
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-11",
                VoltageOut1 = 44,
                ID = 11,
                Relay = new RelayVip("11"),
                RowIndex = 2,
                ColumnIndex = 2
            });
            VipsStand.Add(new Vip
            {
                Name = "Вип-12",
                VoltageOut1 = 44,
                ID = 12,
                Relay = new RelayVip("12"),
                RowIndex = 2,
                ColumnIndex = 3
            });
        }
    }

    public void MultiSetConfigSwitcher(TypePort typePort, string portName, int baud, int stopBits, int parity, int dataBits,
        bool dtr = true)
    {
        foreach (var switcherMeter in SwitchersMetersStand)
        {
            switcherMeter.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
        }
    }


    public async Task<bool> PrimaryCheckDevices()
    {
        //сброс статуса теста
        TestRun = TypeOfTestRun.None;

        //установка статуса теста первичноая провека устройств
        TestRun = TypeOfTestRun.PrimaryCheckDevices;

        if (true)
        {
            //для отладки
            TestCurrentDevice = MultimeterStand;
            MultimeterStand.StatusTest = StatusDeviceTest.Ok;
            //
            PercentCurrentTest = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 20;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 40;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //для отладки
            TestCurrentDevice = SupplyStand;
            SupplyStand.StatusTest = StatusDeviceTest.Ok;
            //
            PercentCurrentTest = 60;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 80;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 100;

            //уведомляем что первичный тест закончен
            TestRun = TypeOfTestRun.PrimaryCheckDevicesReady;

            //сброс текущего проверямего устройства
            TestCurrentDevice = new BaseDevice("0");
            return true;
        }

        //Уведомляем что первичный тест не закончен
        return false;
    }

    public bool ResetCurrentTest()
    {
        //сброс статуса теста
        TestRun = TypeOfTestRun.Stop;
        PercentCurrentTest = 0;

        if (true)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> PrimaryCheckVips()
    {
        //сброс статуса теста
        TestRun = TypeOfTestRun.None;

        //установка тест первичный платок випов 
        TestRun = TypeOfTestRun.PrimaryCheckVips;

        if (true)
        {
            PercentCurrentTest = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //для отладки
            TestCurrentDevice = VipsStand[0].Relay;
            VipsStand[0].Relay.StatusTest = StatusDeviceTest.Ok;
            //
            PercentCurrentTest = 20;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 40;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 60;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //для отладки
            TestCurrentDevice = VipsStand[3].Relay;
            VipsStand[3].Relay.StatusTest = StatusDeviceTest.Ok;
            //
            PercentCurrentTest = 80;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 100;

            TestRun = TypeOfTestRun.PrimaryCheckVipsReady;

            //сброс текущего проверямего устройства
            TestCurrentDevice = new BaseDevice("0");

            return true;
        }

        return false;
    }


    public async Task<bool> MeasurementZero()
    {
        TestRun = TypeOfTestRun.None;

        //Уведомляем что начался тест 0
        TestRun = TypeOfTestRun.MeasurementZero;
        if (true)
        {
            TestRun = TypeOfTestRun.DeviceOperation;
            //
            TestCurrentDevice = BigLoadStand;
            //
            PercentCurrentTest = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //
            TestCurrentDevice = SupplyStand;
            //
            TestRun = TypeOfTestRun.DeviceOperationReady;
            PercentCurrentTest = 20;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            TestRun = TypeOfTestRun.MeasurementZero;
            PercentCurrentTest = 20;
            PercentCurrentTest = 40;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 60;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            TestRun = TypeOfTestRun.DeviceOperation;
            //
            TestCurrentDevice = BigLoadStand;
            //
            PercentCurrentTest = 80;
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            TestRun = TypeOfTestRun.DeviceOperationReady;
            PercentCurrentTest = 100;


            TestCurrentDevice = new BaseDevice("0");
            TestRun = TypeOfTestRun.MeasurementZeroReady;
            return true;
        }

        return false;
    }

    public async Task<bool> WaitSettingToOperatingMode()
    {
        TestRun = TypeOfTestRun.None;
        //Уведомляем что начался выход на режим 
        TestRun = TypeOfTestRun.WaitSettingToOperatingMode;

        if (true)
        {
            //
            TestCurrentDevice = BigLoadStand;
            //
            PercentCurrentTest = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            //
            TestCurrentDevice = SupplyStand;
            //
            PercentCurrentTest = 20;
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            PercentCurrentTest = 40;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 60;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //
            TestCurrentDevice = BigLoadStand;
            //
            PercentCurrentTest = 80;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 100;


            TestCurrentDevice = new BaseDevice("0");
            TestRun = TypeOfTestRun.WaitSettingToOperatingModeReady;
            return true;
        }

        return false;
    }

    public async Task<bool> CyclicMeasurement()
    {
        TestRun = TypeOfTestRun.None;
        //Уведомляем что начался выход на режим 
        TestRun = TypeOfTestRun.CyclicMeasurement;

        int i = 0;
        while (true)
        {
            i++;
            //
            TestCurrentDevice = BigLoadStand;
            //
            PercentCurrentTest = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //
            TestCurrentDevice = SupplyStand;
            //
            PercentCurrentTest = 20;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 40;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 60;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            //
            TestCurrentDevice = BigLoadStand;
            //
            PercentCurrentTest = 80;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            PercentCurrentTest = 100;

            TestCurrentDevice = new BaseDevice("0");
            TestRun = TypeOfTestRun.CyclicMeasurement;

            if (i > 10)
            {
                TestRun = TypeOfTestRun.CyclicMeasurementReady;
                ReportCreate();
                return true;
            }
        }

        return false;
    }

    public void SetTimesTest(TimeSpan all, TimeSpan interval)
    {

        TestAllTime = all;
        TestIntervalTime = interval;

    }

    public void ReportCreate()
    {

    }


    public void SaveReportPlace()
    {
       var txtEditor = "Test";
       var nameFile = "Report ";
        if (FileName != null)
        {
            FileName = string.Empty;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.RestoreDirectory = true;
        saveFileDialog.InitialDirectory= @"Saved Reports";
        saveFileDialog.Filter = "Excel files (*.xmls)|*.xmls";
        var fileNameReplace = nameFile + $"{DateTime.Now}".Replace("/", "-").Replace(":", "-");
        saveFileDialog.FileName = fileNameReplace;
        
        if (saveFileDialog.ShowDialog() == true)
            File.WriteAllText(saveFileDialog.FileName, txtEditor);
        
    }
}

public enum TypeOfTestRun
{
    None,
    Stop,
    PrimaryCheckDevices,
    PrimaryCheckDevicesReady,
    PrimaryCheckVips,
    PrimaryCheckVipsReady,
    DeviceOperation,
    DeviceOperationReady,
    MeasurementZero,
    MeasurementZeroReady,
    WaitSettingToOperatingMode,
    WaitSettingToOperatingModeReady,
    CyclicMeasurement,
    CyclicMeasurementReady,
    CycleWait,
    Error
}