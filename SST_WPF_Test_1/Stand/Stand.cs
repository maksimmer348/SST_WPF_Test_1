using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace SST_WPF_Test_1;

public class Stand : Notify
{
    #region Компоноенты стенда

    private ObservableCollection<BaseDevice> devices = new();

    public ObservableCollection<BaseDevice> Devices
    {
        get => devices;
        set => Set(ref devices, value);
    }

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
            MultimeterStand.SetConfigDevice(TypePort.SerialInput, "COM4", 9600, 1, 0, 8);
            MultimeterStand.ConnectPort += OnCheckConnectPort;
            MultimeterStand.ConnectDevice += OnCheckDevice;
            MultimeterStand.Receive += Receive;

            SupplyStand = new("PSW7-800-2.88") { RowIndex = 0, ColumnIndex = 1 };
            SupplyStand.SetConfigDevice(TypePort.SerialInput, "COM5", 115200, 1, 0, 8);
            SupplyStand.ConnectPort += OnCheckConnectPort;
            SupplyStand.ConnectDevice += OnCheckDevice;
            SupplyStand.Receive += Receive;

            ThermometerStand = new("PSW7-800-2.88") { RowIndex = 0, ColumnIndex = 2 };
            ThermometerStand.SetConfigDevice(TypePort.SerialInput, "COM100", 9600, 1, 0, 8);
            ThermometerStand.ConnectPort += OnCheckConnectPort;
            ThermometerStand.ConnectDevice += OnCheckDevice;
            ThermometerStand.Receive += Receive;

            SmallLoadStand = new("SMLL LOAD-87") { RowIndex = 0, ColumnIndex = 3 };
            SmallLoadStand.SetConfigDevice(TypePort.SerialInput, "COM60", 2400, 1, 0, 8);
            SmallLoadStand.ConnectPort += OnCheckConnectPort;
            SmallLoadStand.ConnectDevice += OnCheckDevice;
            SmallLoadStand.Receive += Receive;

            BigLoadStand = new("BIG LOAD-90") { RowIndex = 0, ColumnIndex = 4 };
            BigLoadStand.SetConfigDevice(TypePort.SerialInput, "COM70", 9600, 1, 0, 8);
            BigLoadStand.ConnectPort += OnCheckConnectPort;
            BigLoadStand.ConnectDevice += OnCheckDevice;
            BigLoadStand.Receive += Receive;

            HeatStand = new("Heat") { RowIndex = 0, ColumnIndex = 5 };
            HeatStand.SetConfigDevice(TypePort.SerialInput, "COM80", 9600, 1, 0, 8);
            HeatStand.ConnectPort += OnCheckConnectPort;
            HeatStand.ConnectDevice += OnCheckDevice;
            HeatStand.Receive += Receive;

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
                switcherMeter.SetConfigDevice(TypePort.SerialInput, "COM90", 9600, 1, 0, 8);
                switcherMeter.ConnectPort += OnCheckConnectPort;
                switcherMeter.ConnectDevice += OnCheckDevice;
                switcherMeter.Receive += Receive;
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


        devices.Add(MultimeterStand);
        devices.Add(SupplyStand);
        devices.Add(ThermometerStand);
        devices.Add(SmallLoadStand);
        devices.Add(BigLoadStand);
        devices.Add(HeatStand);

        foreach (var device in SwitchersMetersStand)
        {
            devices.Add(device);
        }
    }


    #region Прием данных с приборов

    private void Receive(BaseDevice device, string receive)
    {
    }

    private void OnCheckDevice(BaseDevice baseDevice, bool connect)
    {
        PercentCurrentTest += (1 / (float)Devices.Count) * 80;
        if (connect)
        {
            TempVerifiedDevices.Add(baseDevice);
        }
    }

    public void OnCheckConnectPort(BaseDevice baseDevice, bool connect)
    {
        if (connect)
        {
            TempVerifiedDevices.Add(baseDevice);
        }
    }

    #endregion


    /// <summary>
    ///Для настройки одинаковых плат => 1 настройка на 12 плат
    /// </summary>
    /// <param name="typePort"></param>
    /// <param name="portName"></param>
    /// <param name="baud"></param>
    /// <param name="stopBits"></param>
    /// <param name="parity"></param>
    /// <param name="dataBits"></param>
    /// <param name="dtr"></param>
    public void MultiSetConfigSwitcher(TypePort typePort, string portName, int baud, int stopBits, int parity,
        int dataBits,
        bool dtr = true)
    {
        foreach (var switcherMeter in SwitchersMetersStand)
        {
            switcherMeter.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
        }
    }

    #region Инструменты проверки

    public ObservableCollection<BaseDevice> TempVerifiedDevices { get; set; } = new();

    /// <summary>
    /// Проверка на физическое существование порта  
    /// </summary>
    /// <param name="tempCheckDevices"></param>
    /// <param name="delay">Общая задержка проверки (по умолчанию 10)</param>
    /// <returns></returns>
    public async Task<List<BaseDevice>> CheckConnectPorts(List<BaseDevice> tempCheckDevices, int delay = 100)
    {
        //
        PercentCurrentTest = 0;
        //
        foreach (var device in tempCheckDevices)
        {
            device.Close();
        }

        await Task.Delay(TimeSpan.FromMilliseconds(20));

        foreach (var device in tempCheckDevices)
        {
            device.Start();
        }

        await Task.Delay(TimeSpan.FromMilliseconds(100));
        //
        PercentCurrentTest = 20;
        //
        //после задержки в этом списке будут устройства не прошедшие проверку
        var tempErrorDevices = GetErrorDevices(tempCheckDevices);
        return tempErrorDevices;
    }


    /// <summary>
    /// Проверка устройств пингуются ли они
    /// </summary>
    /// <param name="tempCheckDevices"></param>
    /// <param name="externalDelay"></param>
    /// <returns></returns>
    public async Task<List<BaseDevice>> CheckConnectDevices(List<BaseDevice> tempCheckDevices,
        int externalDelay = 0)
    {
        //список для задержек из приборов
        var delaysList = new List<int>();
        //временный список дефетктивынх приборов
        var tempErrorDevices = new List<BaseDevice>();

        foreach (var device in tempCheckDevices)
        {
            //отправляем команду проверки на устройство
            TestCurrentDevice = device;
            device.CheckedConnectDevice();
            delaysList.Add(device.CmdDelay);
        }

        double delay = 0;
        if (externalDelay == 0)
        {
            //используем самую большую задержку из всех проверяемых приборов
            delay = Convert.ToDouble(delaysList.Count > 0 ? delaysList.Max() : 100);
        }
        else
        {
            delay = externalDelay;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        tempErrorDevices = GetErrorDevices(tempCheckDevices);
        PercentCurrentTest = 100;
        return tempErrorDevices;
    }

    private List<BaseDevice> GetErrorDevices(List<BaseDevice> checkedDevices)
    {
        if (!TempVerifiedDevices.Any())
        {
            return checkedDevices.ToList();
        }

        //сравниваем 
        var tempErrorDevices = checkedDevices.Except(TempVerifiedDevices).ToList();
        TempVerifiedDevices.Clear();


        //возвращаем список приборов не прошедших проверку
        return tempErrorDevices;
    }

    #endregion


    #region ПРОВЕРКИ устройств

    /// <summary>
    /// ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА устройств
    /// </summary>
    /// <returns></returns>
    public async Task<bool> PrimaryCheckDevices()
    {
        bool isPrimaryCheckDevices = false;
        //сброс статуса теста
        TestRun = TypeOfTestRun.None;
        //установка статуса теста первичноая провека устройств
        TestRun = TypeOfTestRun.PrimaryCheckDevices;

        int checkCount = 3;
        for (int i = 0; i < checkCount; i++)
        {
            //вставка во временный список список приоров для проверки
            var tempCheckDevices = Devices.ToList();
            //сброс всех статусов
            foreach (var device in tempCheckDevices)
            {
                device.StatusTest = StatusDeviceTest.None;
            }

            //принимает все компорты
            if (tempCheckDevices.Count > 0)
            {
                List<BaseDevice> errorList = await CheckConnectPorts(tempCheckDevices);
                //ждем (если по прношесвтии этого времени в errorPortsList чтот появится значит проверка не прошла)

                //если сбойные компорты есть 
                if (errorList.Any())
                {
                    //вписываем в них ошибку теста
                    foreach (var errorPort in errorList)
                    {
                        errorPort.StatusTest = StatusDeviceTest.Error;
                    }

                    //отбираем прошедшие проверку компорты (сбоыйные порты отброшены)
                    var noErrorPortsList = tempCheckDevices.Except(errorList).ToList();
                    
                    //если такие компорты есть проводим проверку приборов на них на предмет пинга
                    if (noErrorPortsList.Any())
                    {
                        //собираем приборы котороые не ответили на команду статус - сбойные
                        List<BaseDevice> errorDevicesList = await CheckConnectDevices(noErrorPortsList);

                        //елси сбойные утройства есть
                        if (errorDevicesList.Any())
                        {
                            //вписываем в них ошибку теста
                            foreach (var errorDevice in errorDevicesList)
                            {
                                errorDevice.StatusTest = StatusDeviceTest.Error;
                            }
                        }

                        //отбираем нормальные устройства прошедшие и проверку портов и проверку пинга
                        var noErrorList = tempCheckDevices.Except(errorDevicesList.Union(errorList)).ToList();

                        //вписываем в них ок теста
                        foreach (var noErrorDevice in noErrorList)
                        {
                            noErrorDevice.StatusTest = StatusDeviceTest.Ok;
                        }

                        TestCurrentDevice = new BaseDevice("0");
                        return false;
                    }
                   
                }
                //если сбоынйх компортов ВООБЩЕ нет проводим проверку приборов на них на предмет пинга
                else
                {
                    List<BaseDevice> errorDevicesList = await CheckConnectDevices(tempCheckDevices);
                    //если сбоынйу устройства есть
                    if (errorDevicesList.Any())
                    {
                        //вписываем в них ошибку теста
                        foreach (var errorDevice in errorDevicesList)
                        {
                            errorDevice.StatusTest = StatusDeviceTest.Error;
                        }

                        TestCurrentDevice = new BaseDevice("0");
                        return false;
                    }
                    else
                    {
                        foreach (var device in tempCheckDevices)
                        {
                            device.StatusTest = StatusDeviceTest.Ok;
                        }

                        TestRun = TypeOfTestRun.PrimaryCheckDevicesReady;
                        TestCurrentDevice = new BaseDevice("0");
                        return true;
                    }
                }
            }
        }

        PercentCurrentTest = 100;
        TestCurrentDevice = new BaseDevice("0");
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

    #endregion

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
        saveFileDialog.InitialDirectory = @"Saved Reports";
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