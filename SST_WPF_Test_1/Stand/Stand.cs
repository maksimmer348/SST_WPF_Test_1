using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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

    public Currentmeter CurrentmeterStand { get; set; }
    public ThermoVoltmeter ThermoVoltmeterStand { get; set; }
    public Supply SupplyStand { get; set; }
    public SmallLoad SmallLoadStand { get; set; }
    public BigLoad BigLoadStand { get; set; }
    public Heat HeatStand { get; set; }
    public MainRelay MainRelayVip { get; set; }

    private ObservableCollection<BaseDevice> devices = new();

    /// <summary>
    /// Список внещних устройств
    ///</summary>
    public ObservableCollection<BaseDevice> Devices
    {
        get => devices;
        set => Set(ref devices, value);
    }


    private ObservableCollection<BaseDevice> relaysVips = new();

    /// <summary>
    /// Список Релейных лпат Випов
    /// </summary>
    public ObservableCollection<BaseDevice> RelaysVips
    {
        get => relaysVips;
        set => Set(ref relaysVips, value);
    }

    /// <summary>
    /// Список Випов
    /// </summary>
    private ObservableCollection<Vip> vipsPrepareStand = new();

    public ObservableCollection<Vip> VipsPrepareStand
    {
        get => vipsPrepareStand;
        set => Set(ref vipsPrepareStand, value);
    }

    /// <summary>
    /// Список проверяемых Випов
    /// </summary>
    private ObservableCollection<Vip> vipsCheckedStand = new();

    public ObservableCollection<Vip> VipsCheckedStand
    {
        get => vipsCheckedStand;
        set => Set(ref vipsCheckedStand, value);
    }

    public ConfigVips ConfigVip { get; set; } = new ConfigVips();
    public ObservableCollection<TypeVip> TypeVips { get; set; } = new();

    #endregion

    //

    #region Констркутор

    public Stand()
    {
        //SetDevices();
        SetPrepareVips();
    }

    #endregion

    //

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

    private double percentCurrentTest;

    /// <summary>
    /// На соклько процентов выполнен текущий тест
    /// </summary>
    public double PercentCurrentTest
    {
        get => percentCurrentTest;
        set => Set(ref percentCurrentTest, value);
    }

    #endregion

    //

    #region Время испытаний

    public void SetTimesTest(TimeSpan all, TimeSpan interval)
    {
        SetTestAllTime = all;
        SetTestIntervalTime = interval;
    }

    private DateTime testStartTime;

    /// <summary>
    /// Время начала теста
    /// </summary>
    public DateTime TestStartTime
    {
        get => testStartTime;
        set => Set(ref testStartTime, value);
    }

    private DateTime testEndTime;

    /// <summary>
    /// Время окончания теста
    /// </summary>
    public DateTime TestEndTime
    {
        get => testEndTime;
        set => Set(ref testEndTime, value);
    }

    private DateTime nextMeasurementIn;

    /// <summary>
    /// Время следующего замера
    /// </summary>
    public DateTime NextMeasurementIn
    {
        get => nextMeasurementIn;
        set => Set(ref nextMeasurementIn, value);
    }

    private TimeSpan setTestAllTime;

    /// <summary>
    /// Устанлвка сколько будет длится тест
    /// </summary>
    public TimeSpan SetTestAllTime
    {
        get => setTestAllTime;
        set => Set(ref setTestAllTime, value);
    }

    private TimeSpan setTestIntervalTime;

    /// <summary>
    /// Установка через какой интервал времени будет производится замер 
    /// </summary>
    public TimeSpan SetTestIntervalTime
    {
        get => setTestIntervalTime;
        set => Set(ref setTestIntervalTime, value);
    }

    private TimeSpan testLeftEndTime;

    /// <summary>
    /// Время коца теста = DateTime.Now + SetTestAllTime
    /// </summary>
    public TimeSpan TestLeftEndTime
    {
        get => testLeftEndTime;
        set => Set(ref testLeftEndTime, value);
    }

    #endregion

    //

    #region Отчет

    private string fileName;

    /// <summary>
    /// Создание отчета
    /// </summary>
    public void ReportCreate()
    {
    }

    /// <summary>
    /// Путь к файлу где будет хранится отчет
    /// </summary>
    public string FileName
    {
        get => fileName;
        set => Set(ref fileName, value);
    }

    /// <summary>
    /// Сохранение отчета по пути
    /// </summary>
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

    #endregion

    #region Вспомогательные методы

    CancellationTokenSource ctsCheckDevice = new();
    CancellationTokenSource ctsReceiveDevice = new();

    /// <summary>
    /// Сброс текущего проверяемого устройства и процента теста
    /// </summary>
    void ResetTestDeviceAndPercent()
    {
        PercentCurrentTest = 100;
        TestCurrentDevice = new BaseDevice("0");
    }

    /// <summary>
    /// Подпись на события из устройств
    /// </summary>
    public void InvokeDevices()
    {
        //TODO споросить тему
        foreach (var device in Devices)
        {
            device.ConnectPort = OnCheckConnectPort;
            device.ConnectDevice = OnCheckDevice;
            device.Receive = Receive;
        }

        //TODO раскоменить елси чтот пойдет не так
        foreach (var relayVip in RelaysVips)
        {
            relayVip.ConnectPort = OnCheckConnectPort;
            relayVip.ConnectDevice = OnCheckDevice;
            relayVip.Receive = Receive;
        }

        MainRelayVip.ConnectPort = OnCheckConnectPort;
        MainRelayVip.ConnectDevice = OnCheckDevice;
        MainRelayVip.Receive = Receive;
    }

    /// <summary>
    /// Установка приборов по умолчанию (для тестов)
    /// </summary>
    public void SetDevicesi()
    {
        if (true) //TODO (true) - если сеарилизатор недосутпен выводим исключение и создаем приборы со станрдартными настройками
        {
            CurrentmeterStand = new("GDM-78255A") { RowIndex = 0, ColumnIndex = 0 };
            CurrentmeterStand.SetConfigDevice(TypePort.SerialInput, "COM8", 115200, 1, 0, 8);
            CurrentmeterStand.ConnectPort += OnCheckConnectPort;
            CurrentmeterStand.ConnectDevice += OnCheckDevice;
            CurrentmeterStand.Receive += Receive;
            Devices.Add(CurrentmeterStand);


            ThermoVoltmeterStand = new("GDM-78255A") { RowIndex = 0, ColumnIndex = 2 };
            ThermoVoltmeterStand.SetConfigDevice(TypePort.SerialInput, "COM7", 115200, 1, 0, 8);
            ThermoVoltmeterStand.ConnectPort += OnCheckConnectPort;
            ThermoVoltmeterStand.ConnectDevice += OnCheckDevice;
            ThermoVoltmeterStand.Receive += Receive;
            Devices.Add(ThermoVoltmeterStand);

            SupplyStand = new("PSW7-800-2.88") { RowIndex = 0, ColumnIndex = 1 };
            SupplyStand.SetConfigDevice(TypePort.SerialInput, "COM5", 115200, 1, 0, 8);
            SupplyStand.ConnectPort += OnCheckConnectPort;
            SupplyStand.ConnectDevice += OnCheckDevice;
            SupplyStand.Receive += Receive;
            Devices.Add(SupplyStand);

            //TODO вернуть 
            // SmallLoadStand = new("SMLL LOAD-87") { RowIndex = 0, ColumnIndex = 3 };
            // SmallLoadStand.SetConfigDevice(TypePort.SerialInput, "COM60", 2400, 1, 0, 8);
            // SmallLoadStand.ConnectPort += OnCheckConnectPort;
            // SmallLoadStand.ConnectDevice += OnCheckDevice;
            // SmallLoadStand.Receive += Receive;
            //Devices.Add(SmallLoadStand);

            BigLoadStand = new("AFG-72112") { RowIndex = 0, ColumnIndex = 4 };
            BigLoadStand.SetConfigDevice(TypePort.SerialInput, "COM6", 115200, 1, 0, 8);
            BigLoadStand.ConnectPort += OnCheckConnectPort;
            BigLoadStand.ConnectDevice += OnCheckDevice;
            BigLoadStand.Receive += Receive;
            Devices.Add(BigLoadStand);

            //TODO вернуть 
            // HeatStand = new("Heat") { RowIndex = 0, ColumnIndex = 5 };
            // HeatStand.SetConfigDevice(TypePort.SerialInput, "COM80", 9600, 1, 0, 8);
            // HeatStand.ConnectPort += OnCheckConnectPort;
            // HeatStand.ConnectDevice += OnCheckDevice;
            // HeatStand.Receive += Receive;
            //Devices.Add(HeatStand);

            //TODO вернуть 
            // Devices.Add(new SwitcherMeter("1") { RowIndex = 1, ColumnIndex = 0 });
            // Devices.Add(new SwitcherMeter("2") { RowIndex = 1, ColumnIndex = 1 });
            // Devices.Add(new SwitcherMeter("3") { RowIndex = 1, ColumnIndex = 2 });
            // Devices.Add(new SwitcherMeter("4") { RowIndex = 1, ColumnIndex = 3 });
            // Devices.Add(new SwitcherMeter("5") { RowIndex = 1, ColumnIndex = 4 });
            // Devices.Add(new SwitcherMeter("6") { RowIndex = 1, ColumnIndex = 5 });
            // Devices.Add(new SwitcherMeter("7") { RowIndex = 2, ColumnIndex = 0 });
            // Devices.Add(new SwitcherMeter("8") { RowIndex = 2, ColumnIndex = 1 });
            // Devices.Add(new SwitcherMeter("9") { RowIndex = 2, ColumnIndex = 2 });
            // Devices.Add(new SwitcherMeter("10") { RowIndex = 2, ColumnIndex = 3 });
            // Devices.Add(new SwitcherMeter("11") { RowIndex = 2, ColumnIndex = 4 });
            // Devices.Add(new SwitcherMeter("12") { RowIndex = 2, ColumnIndex = 5 });

            //TODO вернуть 
            // foreach (var switcherMeter in Devices)
            // {
            //     if(switcherMeter is SwitcherMeter)
            //     switcherMeter.SetConfigDevice(TypePort.SerialInput, "COM2", 9600, 1, 0, 8);
            //     switcherMeter.ConnectPort += OnCheckConnectPort;
            //     switcherMeter.ConnectDevice += OnCheckDevice;
            //     switcherMeter.Receive += Receive;
            // }

            RelaysVips.Add(new RelayVip("1"));
            RelaysVips.Add(new RelayVip("2"));
            RelaysVips.Add(new RelayVip("3"));
            RelaysVips.Add(new RelayVip("4"));
            RelaysVips.Add(new RelayVip("5"));
            RelaysVips.Add(new RelayVip("6"));
            RelaysVips.Add(new RelayVip("7"));
            RelaysVips.Add(new RelayVip("8"));
            RelaysVips.Add(new RelayVip("9"));
            RelaysVips.Add(new RelayVip("10"));
            RelaysVips.Add(new RelayVip("11"));
            RelaysVips.Add(new RelayVip("12"));
            //TODO вернуть 
            foreach (var relay in RelaysVips)
            {
                relay.SetConfigDevice(TypePort.SerialInput, "COM3", 9600, 1, 0, 8);
                relay.ConnectPort += OnCheckConnectPort;
                relay.ConnectDevice += OnCheckDevice;
                relay.Receive += Receive;
            }
        }
    }


    void SetPrepareVips()
    {
        VipsPrepareStand = new();
        VipsPrepareStand.Add(new Vip(1)
        {
            RowIndex = 0,
            ColumnIndex = 0,
        });
        VipsPrepareStand.Add(new Vip(2)
        {
            RowIndex = 0,
            ColumnIndex = 1
        });
        VipsPrepareStand.Add(new Vip(3)
        {
            RowIndex = 0,
            ColumnIndex = 2
        });
        VipsPrepareStand.Add(new Vip(4)
        {
            RowIndex = 0,
            ColumnIndex = 3
        });
        VipsPrepareStand.Add(new Vip(5)
        {
            RowIndex = 1,
            ColumnIndex = 0
        });
        VipsPrepareStand.Add(new Vip(6)
        {
            RowIndex = 1,
            ColumnIndex = 1
        });
        VipsPrepareStand.Add(new Vip(7)
        {
            RowIndex = 1,
            ColumnIndex = 2
        });
        VipsPrepareStand.Add(new Vip(8)
        {
            RowIndex = 1,
            ColumnIndex = 3
        });
        VipsPrepareStand.Add(new Vip(9)
        {
            RowIndex = 2,
            ColumnIndex = 0
        });
        VipsPrepareStand.Add(new Vip(10)
        {
            RowIndex = 2,
            ColumnIndex = 1
        });
        VipsPrepareStand.Add(new Vip(11)
        {
            RowIndex = 2,
            ColumnIndex = 2
        });
        VipsPrepareStand.Add(new Vip(12)
        {
            RowIndex = 2,
            ColumnIndex = 3
        });

        //Добавляем предустановленные типы випов

        //TODO Serialize comment
        // ConfigVip.PrepareAddTypeVips();
        //TODO Serialize comment

        TypeVips = ConfigVip.TypeVips;
    }

    /// <summary>
    /// Предварительное добавление каждой релейной платы к соответвующему випу
    /// </summary>
    public void AddRelayToVip()
    {
        for (var index = 0; index < VipsPrepareStand.Count; index++)
        {
            var vip = VipsPrepareStand[index];
            vip.Relay = RelaysVips[index] as RelayVip;
        }

        InitMainRelayVip();
    }

    public void InitMainRelayVip()
    {
        MainRelayVip = new("MainRelay", RelaysVips);
        var config = RelaysVips[0].GetConfigDevice();
        MainRelayVip.SetConfigDevice(config.TypePort, config.PortName, config.Baud, config.StopBits, config.Parity,
            config.DataBits);
    }

    /// <summary>
    /// Установка типа випов
    /// </summary>
    /// <param name="selectTypeVip">ВЫбранный тип Випа</param>
    public void SetTypeVips(TypeVip selectTypeVip)
    {
        foreach (var vip in VipsPrepareStand)
        {
            vip.Type = selectTypeVip;
        }
    }

    /// <summary>
    /// Если у випа отстутвует номер его не тестировать
    /// </summary>
    public void SetIsTestedVips()
    {
        foreach (var vip in VipsPrepareStand)
        {
            vip.StatusTest = StatusDeviceTest.None;

            if (!string.IsNullOrWhiteSpace(vip.Number))
            {
                vip.IsTested = true;
            }
            else
            {
                vip.IsTested = false;
            }
        }
    }

    /// <summary>
    ///Для настройки одинаковых плат коммутации => 1 настройка на 12 плат
    /// </summary>
    /// <param name="typePort">Тип исопльзуемой библиотеки com port</param>
    /// <param name="portName">омер компорта</param>
    /// <param name="baud">Бауд рейт компорта</param>
    /// <param name="stopBits">Стоповые биты компорта</param>
    /// <param name="parity">Parity bits</param>
    /// <param name="dataBits">Data bits count</param>
    /// <param name="dtr"></param>
    public void MultiSetConfigSwitcher(TypePort typePort, string portName, int baud, int stopBits, int parity,
        int dataBits,
        bool dtr = true)
    {
        foreach (var switcherMeter in Devices)
        {
            if (switcherMeter is SwitcherMeter s)
            {
                s.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
            }
        }
    }

    /// <summary>
    ///Для настройки одинаковых плат реле в Випах => 1 настройка на 12 плат
    /// </summary>
    /// <param name="typePort">Тип исопльзуемой библиотеки com port</param>
    /// <param name="portName">омер компорта</param>
    /// <param name="baud">Бауд рейт компорта</param>
    /// <param name="stopBits">Стоповые биты компорта</param>
    /// <param name="parity">Parity bits</param>
    /// <param name="dataBits">Data bits count</param>
    /// <param name="dtr"></param>
    public void MultiSetConfigRelayVip(TypePort typePort, string portName, int baud, int stopBits, int parity,
        int dataBits,
        bool dtr = true)
    {
        foreach (var relay in RelaysVips)
        {
            relay.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
        }
    }

    public void ConfigMainRelay(TypePort typePort, string portName, int baud, int stopBits, int parity, int dataBits,
        bool dtr)
    {
        MainRelayVip.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
    }

    #endregion

    //

    #region Обработка событий с приборов

    /// <summary>
    /// Событие поверки порта на коннект 
    /// </summary>/// <param name="baseDevice"></param>
    /// <param name="connect"></param>
    public void OnCheckConnectPort(BaseDevice baseDevice, bool connect)
    {
        if (connect)
        {
            //Debug.WriteLine(baseDevice.Name);
            TempVerifiedDevices.Add(baseDevice);
        }
    }

    /// <summary>
    /// Событие проверки устройства на коннект
    /// </summary>
    /// <param name="baseDevice"></param>
    /// <param name="connect"></param>
    private void OnCheckDevice(BaseDevice baseDevice, bool connect)
    {
        TestCurrentDevice = baseDevice;
        if (connect)
        {
            TempVerifiedDevices.Add(baseDevice);
        }

        if (baseDevice is not RelayVip)
        {
            PercentCurrentTest += ((1 / (float)Devices.Count) * 60);

            //сраниваем списки
            var ss = Devices.Except(TempVerifiedDevices).ToList();

            if (!ss.Any())
            {
                ctsCheckDevice.Cancel();
            }
        }

        if (baseDevice is RelayVip or MainRelay)
        {
            PercentCurrentTest += ((1 / (float)RelaysVips.Count) * 60);
        }
    }

    private Dictionary<BaseDevice, List<string>> ReceiveInDevice = new Dictionary<BaseDevice, List<string>>();

    /// <summary>
    /// Событие приема данных из прибора
    /// </summary>
    /// <param name="device">Прибор посылающий данные</param>
    /// <param name="receive">Данные</param>
    private void Receive(BaseDevice device, string receive)
    {
        Debug.WriteLine($"{device.Name}/{receive}");

        if (!ReceiveInDevice.ContainsKey(device))
        {
            ReceiveInDevice.Add(device, new List<string>());
            ctsReceiveDevice.Cancel();
        }

        ReceiveInDevice[device].Add(receive);
    }

    #endregion

    //

    #region Инструменты проверки

    public ObservableCollection<BaseDevice> TempVerifiedDevices { get; set; } = new();

    /// <summary>
    /// Проверка на физическое существование портов  
    /// </summary>
    /// <param name="tempCheckDevices">Временный списко устройств</param>
    /// <param name="delay">Общая задержка проверки (по умолчанию 100)</param>
    /// <returns></returns>
    public async Task<List<BaseDevice>> CheckConnectPorts(List<BaseDevice> tempCheckDevices, int delay = 100)
    {
        //
        PercentCurrentTest = 0;
        //
        if (tempCheckDevices.All(x => x is RelayVip))
        {
            //тк все релейны платы висят на одном компорту проверяем только маин реле
            MainRelayVip.Close();
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            MainRelayVip.Start();
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            //
            PercentCurrentTest = 20;
            //

            if (TempVerifiedDevices.Contains(MainRelayVip))
            {
                return new List<BaseDevice>();
            }

            //если 
            return tempCheckDevices;
        }

        foreach (var device in tempCheckDevices)
        {
            device.Close();
        }

        await Task.Delay(TimeSpan.FromMilliseconds(100));

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
    /// <param name="tempCheckDevices">Временный списко устройств</param>
    /// <param name="token">Сброс вермени ожидания если прибор ответил раньше</param>
    /// <param name="externalDelay">Общая задержка проверки (по умолчанию 0)</param>
    /// <returns></returns>
    public async Task<List<BaseDevice>> CheckConnectDevices(List<BaseDevice> tempCheckDevices,
        CancellationToken token, int externalDelay = 0)
    {
        //сброс временного списка дефетктивынх приборов
        List<BaseDevice> tempErrorDevices = new List<BaseDevice>();
        try
        {
            if (tempCheckDevices.All(x => x is RelayVip))
            {
                foreach (var device in tempCheckDevices)
                {
                    TestCurrentDevice = device;
                    //отправляем команду проверки на устройство
                    MainRelayVip.CheckedConnectRelay(device);
                    //ждем
                    await Task.Delay(TimeSpan.FromMilliseconds(300));
                }

                //после задержки в этом списке будут устройства не прошедшие проверку
                tempErrorDevices = GetErrorDevices(tempCheckDevices);
            }
            else
            {
                foreach (var device in tempCheckDevices)
                {
                    //отправляем команду проверки на устройство
                    TestCurrentDevice = device;
                    device.CheckedConnectDevice();
                }

                //ждем
                await Task.Delay(TimeSpan.FromMilliseconds(800), ctsCheckDevice.Token);

                //после задержки в этом списке будут устройства не прошедшие проверку
                tempErrorDevices = GetErrorDevices(tempCheckDevices);
            }
        }
        //елси задлаче была прервана заранее полняем следующий код
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            //после прерывания задрежки в этом списке будут устройства не прошедшие проверку
            tempErrorDevices = GetErrorDevices(tempCheckDevices);
            ctsCheckDevice = new CancellationTokenSource();
            return tempErrorDevices;
        }
        catch (Exception e)
        {
            throw new StandException($"StandException: ошибка  \"{e.Message})]\" при проверке устройств");
        }

        PercentCurrentTest = 100;
        return tempErrorDevices;
    }

    /// <summary>
    /// Получение списка сбойных приборов
    /// </summary>
    /// <param name="checkedDevices">Временный список устройств</param>
    /// <returns></returns>
    private List<BaseDevice> GetErrorDevices(List<BaseDevice> checkedDevices)
    {
        if (!TempVerifiedDevices.Any())
        {
            return checkedDevices.ToList();
        }

        //сравниваем временный список утсройств со списком сформировванным из отвветивших приборов
        //и кладем в список сбойных устройств
        var tempErrorDevices = checkedDevices.Except(TempVerifiedDevices).ToList();

        //очищаем список ответивших приборов
        TempVerifiedDevices.Clear();

        //возвращаем список приборов не прошедших проверку
        return tempErrorDevices;
    }

    /// <summary>
    /// Получение списка проверяемых релейных плат из выбранных Випов 
    /// </summary>
    /// <param name="tempCheckVips"></param>
    /// <returns></returns>
    private List<BaseDevice> GetRelayInVips(IEnumerable<Vip> tempCheckVips)
    {
        var tempRelays = new List<BaseDevice>();

        foreach (var vip in tempCheckVips)
        {
            tempRelays.Add(vip.Relay);
        }

        return tempRelays;
    }

    #endregion

    //

    #region Предварительные проверки устройств

    /// <summary>
    /// Сброс/остановить/отменить проверки устройств 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ResetCurrentTest()
    {
        //сброс петель проверок
        isTestRun = false;
        //сброс всех токенов
        ctsCheckDevice.Cancel();
        ctsReceiveDevice.Cancel();
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        //увдеомление формы что мы сбросили испытания
        TestRun = TypeOfTestRun.Stop;
        ResetTestDeviceAndPercent();
        return true;
    }

    /// <summary>
    /// ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА устройств
    /// </summary>
    /// <returns>Результат предаварительной проверки</returns>
    public async Task<bool> PrimaryCheckDevices()
    {
        //сброс статуса теста
        TestRun = TypeOfTestRun.None;
        //установка статуса теста первичноая провека устройств
        TestRun = TypeOfTestRun.PrimaryCheckDevices;
        var check = await CheckBaseDevices(Devices.ToList());
        if (check)
        {
            TestRun = TypeOfTestRun.PrimaryCheckDevicesReady;
            return check;
        }

        TestRun = TypeOfTestRun.Error;

        return check;
    }

    /// <summary>
    /// Предварительная проверка випов
    /// </summary>
    /// <returns></returns>
    public async Task<bool> PrimaryCheckVips()
    {
        //сброс статуса теста
        TestRun = TypeOfTestRun.None;

        //установка тест первичный платок випов 
        TestRun = TypeOfTestRun.PrimaryCheckVips;

        foreach (var relayVip in RelaysVips)
        {
            relayVip.StatusTest = StatusDeviceTest.None;
        }

        //предварительная настройка тестрировать ли вип => если у Випа есть имя то тестировать
        SetIsTestedVips();
        //вставка во временный список список Випов для проверки платок
        var tempCheckVips = VipsPrepareStand.Where(x => x.IsTested).ToList();
        //временный список для проверяемых реле
        var tempRelays = GetRelayInVips(tempCheckVips);

        var check = await CheckBaseDevices(tempRelays);

        if (check)
        {
            VipsCheckedStand = new ObservableCollection<Vip>(tempCheckVips);
            PercentCurrentTest = 100;
            TestRun = TypeOfTestRun.PrimaryCheckVipsReady;
        }

        PercentCurrentTest = 100;
        return check;
    }


    private bool isTestRun = true;

    //TODO вынести в конфиг checkCountAll (возможно)
    /// <summary>
    /// Проверка компорта и ответа от любого устройства на команду Статус
    /// </summary>
    /// <param name="tempCheckDevices">Список устройств</param>
    /// <param name="checkCountAll">Колво попыток достучатся до устройств если они не отвечают</param>
    /// <returns></returns>
    public async Task<bool> CheckBaseDevices(List<BaseDevice> tempCheckDevices, int checkCountAll = 3)
    {
        int checkCountCurrent = 1;
        isTestRun = true;
        while (isTestRun)
        {
            PercentCurrentTest = 0;
            checkCountCurrent++;

            //вставка во временный список список приоров для проверки
            //var tempCheckDevices = Devices.ToList();

            //сброс всех статусов
            foreach (var device in tempCheckDevices)
            {
                device.StatusTest = StatusDeviceTest.None;
            }

            //принимает все компорты
            if (tempCheckDevices.Count > 0)
            {
                //сброс временого списка сбоынйх компортов
                List<BaseDevice> errorPortList = new List<BaseDevice>();

                //если это первая попытка проверки то  
                if (checkCountCurrent > 1)
                {
                    //ждем (если по прношесвтии этого времени в errorPortsList чтот появится значит проверка порта не прошла)
                    errorPortList = await CheckConnectPorts(tempCheckDevices);
                }

                //если сбойные компорты есть 
                if (errorPortList.Any())
                {
                    //вписываем в них ошибку теста
                    foreach (var errorPort in errorPortList)
                    {
                        errorPort.StatusTest = StatusDeviceTest.Error;
                    }

                    //отбираем прошедшие проверку компорты (сбоыйные порты отброшены)
                    var noErrorPortsList = tempCheckDevices.Except(errorPortList).ToList();

                    //если такие компорты есть проводим проверку приборов на них на предмет пинга
                    if (noErrorPortsList.Any())
                    {
                        //собираем приборы котороые не ответили на команду статус - сбойные
                        List<BaseDevice> errorDevicesList =
                            await CheckConnectDevices(noErrorPortsList, ctsCheckDevice.Token);

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
                        var noErrorList = tempCheckDevices.Except(errorDevicesList.Union(errorPortList)).ToList();

                        //вписываем в них ок теста
                        foreach (var device in noErrorList)
                        {
                            device.StatusTest = StatusDeviceTest.Ok;
                        }

                        //
                        ResetTestDeviceAndPercent();
                        //

                        // если количетво попыток больше устновленных выходим из петли с false
                        if (checkCountCurrent > checkCountAll)
                        {
                            return false;
                        }
                    }
                }
                //если сбоынйх компортов ВООБЩЕ нет проводим проверку приборов на них на предмет пинга
                else
                {
                    //отбор сбоынйх устройств
                    List<BaseDevice> errorDevicesList =
                        await CheckConnectDevices(tempCheckDevices, ctsCheckDevice.Token);

                    //если сбоынйу устройства есть
                    if (errorDevicesList.Any())
                    {
                        //вписываем в них ошибку теста
                        foreach (var errorDevice in errorDevicesList)
                        {
                            errorDevice.StatusTest = StatusDeviceTest.Error;
                        }

                        //отбираем прошедшие проверку устройства (сбоыйные устройства отброшены)
                        var noErrorDeviceList = tempCheckDevices.Except(errorDevicesList).ToList();

                        //вписываем в них ок теста
                        foreach (var noErrorDevice in noErrorDeviceList)
                        {
                            noErrorDevice.StatusTest = StatusDeviceTest.Ok;
                        }

                        //если количетво попыток больше устновленных выходим из петли с false
                        if (checkCountCurrent > checkCountAll)
                        {
                            //
                            ResetTestDeviceAndPercent();
                            //
                            return false;
                        }

                        //
                        ResetTestDeviceAndPercent();
                        //
                    }
                    else
                    {
                        foreach (var device in tempCheckDevices)
                        {
                            device.StatusTest = StatusDeviceTest.Ok;
                        }

                        //
                        ResetTestDeviceAndPercent();
                        //
                        return true;
                    }
                }
            }

            if (checkCountCurrent > checkCountAll)
            {
                //
                ResetTestDeviceAndPercent();
                //
                return false;
            }
        }

        //
        ResetTestDeviceAndPercent();
        //
        return false;
    }

    #endregion

    //

    #region Инструменты отправки/приема команд в приборы

    /// <summary>
    /// Получение параметров приборов из типа Випа
    /// </summary>
    /// <returns>DeviceParameters</returns>
    DeviceParameters GetParameterForDevice()
    {
        return vipsPrepareStand[0].Type.GetDeviceParameters();
    }

    private async Task WriteCommand(BaseDevice device, string cmd, string parameter = null,
        CancellationToken token = default)
    {
        try
        {
            device.TransmitCmdInLib(cmd, parameter);
            await Task.Delay(TimeSpan.FromMilliseconds(100), ctsReceiveDevice.Token);
        }
        //елси задлаче была прервана заранее полняем следующий код
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return;
        }
        catch (Exception e)
        {
            throw new StandException($"StandException: ошибка  \"{e.Message})]\" при предаче данных устройству");
        }
    }

    /// <summary>
    ///  Запрос и проверка на правильный ответ от прибора
    /// </summary>
    /// <param name="tempChecks">Список правильных ответов (если прибор ответил верно item = true)</param>
    /// <param name="device">Проверяемый прибор</param>
    /// <param name="cmd">Стандартная команда из библиотеки</param>
    /// <param name="parameter">Параметр команды из типа Випа</param>
    /// <param name="token">Сброс вермени ожидания если прибор ответил раньше</param>
    /// <exception cref="StandException"></exception>
    private async Task<bool> WriteReadCommands(BaseDevice device, string cmd,
        string parameter = null, TempChecks tempChecks = null,
        CancellationToken token = default)
    {
        string receiveInLib = null;
        bool matches = false;

        if (device is not RelayVip)
        {
            try
            {
                //запрос в прибор команды
                receiveInLib = device.TransmitCmdInLib(cmd);

                await Task.Delay(TimeSpan.FromMilliseconds(200), ctsReceiveDevice.Token);

                //данные из листа приема от устройств
                var receive = ReceiveInDevice[device];


                if (receiveInLib != null && !string.IsNullOrWhiteSpace(receiveInLib))
                {
                    //проверка листа приема на содержание в ответе от прибора параметра команды -
                    //берется из Recieve библиотеки
                    matches = CastToNormalValues(receive.Last()).Contains(receiveInLib);
                }
                else
                {
                    //проверка листа приема на содержание в ответе от прибора параметра команды
                    matches = CastToNormalValues(receive.Last()).Contains(parameter);
                }

                //очистка листа приема от устроойств
                ReceiveInDevice[device].Clear();
                //добавление результата проверки в список проверки
                tempChecks?.Add(matches);
                return matches;
            }
            //елси задлаче была прервана заранее полняем следующий код
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                ctsReceiveDevice = new CancellationTokenSource();
                var receive = ReceiveInDevice[device];

                if (receiveInLib != null && !string.IsNullOrWhiteSpace(receiveInLib))
                {
                    //проверка листа приема на содержание в ответе от прибора параметра команды -
                    //берется из Recieve библиотеки
                    matches = CastToNormalValues(receive.Last()).Contains(receiveInLib);
                }
                else
                {
                    //проверка листа приема на содержание в ответе от прибора параметра команды
                    matches = CastToNormalValues(receive.Last()).Contains(parameter);
                }

                ReceiveInDevice[device].Clear();
                tempChecks?.Add(matches);
                return matches;
            }
            catch (Exception e)
            {
                //если вылеатет какоето исключение записываем в лист провеки false
                tempChecks?.Add(false);
                throw new StandException($"StandException: ошибка \"{e.Message})]\" при проверке данных с устройства");
            }
        }

        if (device is RelayVip relay)
        {
            //запрос в прибор команды
            receiveInLib = relay.TransmitCmdInLib(cmd);
        }

        return false;
    }

    /// <summary>
    /// Преобразовние строк вида "SQU +2.00000000E+02,+4.000E+00,+2.00E+00" в стандартные строки вида 200, 4, 20
    /// </summary>
    /// <param name="str">Строка которая будет преобразована</param>
    /// <returns></returns>
    public string CastToNormalValues(string str)
    {
        if (str != null)
        {
            decimal myDecimalValue = 0;

            if (str.Contains("E+"))
            {
                myDecimalValue = Decimal.Parse(str, System.Globalization.NumberStyles.Float);
                return myDecimalValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        return str;
    }

    /// <summary>
    /// Включение/выключение устройства
    /// </summary>
    /// <param name="device">Устройство</param>
    /// <param name="values">Ответ который утройство должно отправить в ответ на запрос output</param>
    /// <param name="on">true - вкл, false - выкл</param>
    /// <returns>Результат включения/выключение</returns>
    private async Task<bool> OutputDevice(BaseDevice device, BaseDeviceValues values, bool on = true)
    {
        if (on)
        {
            //если выход выкл 
            if (await WriteReadCommands(device, "Get output", values.OutputOff,
                    token: ctsReceiveDevice.Token))
            {
                //делаем выход вкл
                await WriteCommand(device, "Set output on");
            }

            //если выход был или стал вкл продолжаем
            if (await WriteReadCommands(device, "Get output", values.OutputOn,
                    token: ctsReceiveDevice.Token))
            {
                return true;
            }
        }
        else
        {
            //если выход вкл 
            if (await WriteReadCommands(device, "Get output", values.OutputOn,
                    token: ctsReceiveDevice.Token))
            {
                //делаем выход выкл
                await WriteCommand(device, "Set output off");
            }

            //если выход был или стал вкл продолжаем
            if (await WriteReadCommands(device, "Get output", values.OutputOff,
                    token: ctsReceiveDevice.Token))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    //

    #region Замеры

    Stopwatch measurementTimer = new();

    /// <summary>
    /// 0 замер
    /// </summary>
    /// <returns>Результат 0 замера</returns>
    public async Task<bool> MeasurementZero()
    {
        TestRun = TypeOfTestRun.None;
        //Уведомляем что начался тест 0
        TestRun = TypeOfTestRun.MeasurementZero;
        //уведопляем что идет работа с приобором

        //вытаскиваем из списка приборов - прибор нужного типа (BigLoad)
        BigLoadStand = Devices.GetTypeDevice<BigLoad>();
        //статус проверяемого прибора для вьюмодели
        TestCurrentDevice = BigLoadStand;
        TestRun = TypeOfTestRun.DeviceOperation;

        //если прибор был вчелючен выключим его
        await OutputDevice(BigLoadStand, GetParameterForDevice().BigLoadValues, false);

        //запрос на установку параметров генератора/большой нагрузки
        await WriteCommand(BigLoadStand, "Set freq", GetParameterForDevice().BigLoadValues.Freq);
        await WriteCommand(BigLoadStand, "Set ampl", GetParameterForDevice().BigLoadValues.Ampl);
        await WriteCommand(BigLoadStand, "Set dco", GetParameterForDevice().BigLoadValues.Dco);
        await WriteCommand(BigLoadStand, "Set squ", GetParameterForDevice().BigLoadValues.Squ);

        //
        PercentCurrentTest = 10;
        //

        //правильно ли были установлены пармтеры генератора/большой нагрузки
        TempChecks t = TempChecks.Start();
        //если прибор ответил правильно в t будет записан true
        await WriteReadCommands(BigLoadStand, "Get freq", GetParameterForDevice().BigLoadValues.Freq, t,
            ctsReceiveDevice.Token);
        await WriteReadCommands(BigLoadStand, "Get ampl", GetParameterForDevice().BigLoadValues.Ampl, t,
            ctsReceiveDevice.Token);
        await WriteReadCommands(BigLoadStand, "Get dco", GetParameterForDevice().BigLoadValues.Dco, t,
            ctsReceiveDevice.Token);
        await WriteReadCommands(BigLoadStand, "Get squ", GetParameterForDevice().BigLoadValues.Squ, t,
            ctsReceiveDevice.Token);
        PercentCurrentTest = 20;

        //проверяем правильные ответы от приборов с помозтб класса t и пытаемся влючить устройство если все ок вернет тру
        if (t.IsOk && await OutputDevice(BigLoadStand, GetParameterForDevice().BigLoadValues))
        {
            //
            PercentCurrentTest = 30;
            //

            SupplyStand = Devices.GetTypeDevice<Supply>();
            TestRun = TypeOfTestRun.DeviceOperation;
            //статус проверяемого прибора для вьюмодели
            TestCurrentDevice = SupplyStand;
            //если прибор был включен выключим его
            await OutputDevice(SupplyStand, GetParameterForDevice().BigLoadValues, false);
            await WriteCommand(SupplyStand, "Set volt", GetParameterForDevice().SupplyValues.Voltage);
            await WriteCommand(SupplyStand, "Set curr", GetParameterForDevice().SupplyValues.Current);

            t = TempChecks.Start();
            await WriteReadCommands(SupplyStand, "Get volt", GetParameterForDevice().SupplyValues.Voltage,
                t, ctsReceiveDevice.Token);
            await WriteReadCommands(SupplyStand, "Get curr", GetParameterForDevice().SupplyValues.Current,
                t, ctsReceiveDevice.Token);
            //
            PercentCurrentTest = 50;
            //
            measurementTimer.Start();
            //если прибор был выключен включим его
            if (t.IsOk && await OutputDevice(SupplyStand, GetParameterForDevice().BigLoadValues))
            {
                Debug.WriteLine($"Time suplly on {measurementTimer.Elapsed.Milliseconds}");
                //ждем время согласно документации
                TestRun = TypeOfTestRun.WaitSupplyMeasurementZero;

                ThermoVoltmeterStand = Devices.GetTypeDevice<ThermoVoltmeter>();
                //статус проверяемого прибора для вьюмодели
                TestCurrentDevice = ThermoVoltmeterStand;
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                await WriteCommand(ThermoVoltmeterStand, "Set volt meter",
                    GetParameterForDevice().ThermoVoltmeterValues.VoltageMaxLimit);
                t = TempChecks.Start();
                await WriteReadCommands(ThermoVoltmeterStand, "Get volt meter",
                    GetParameterForDevice().ThermoVoltmeterValues.VoltageMaxLimit,
                    t, ctsReceiveDevice.Token);
                await WriteReadCommands(ThermoVoltmeterStand, "Get func",
                    GetParameterForDevice().ThermoVoltmeterValues.VoltageMaxLimit,
                    t, ctsReceiveDevice.Token);
                TestRun = TypeOfTestRun.WaitSupplyMeasurementZeroReady;
                if (t.IsOk)
                {
                    EnabledRelayVips(VipsCheckedStand);
                    return true;
                }
            }
        }
        ResetTestDeviceAndPercent();
        return false;
    }

    public async Task<bool> EnabledRelayVips(ObservableCollection<Vip> vipsCheckedStand)
    {
        TempChecks t = TempChecks.Start();
        foreach (var vip in vipsCheckedStand)
        {
            await WriteReadCommands(vip.Relay, "On", tempChecks: t, token:ctsReceiveDevice.Token);
        }
        if (t.IsOk)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ожидание нагрева плиты и выход на режим
    /// </summary>
    /// <returns>Результат нагрева плиты в заданное время/или нет</returns>
    public async Task<bool> WaitForTestMode()
    {
        ResetTestDeviceAndPercent();
        return false;
        //TODO раскомменить когда добавится нагреватель
    }

    /// <summary>
    /// Цикл замеров
    /// </summary>
    /// <returns>Успешен ли цикл/не сбойнул ли какойто прибор</returns>
    public async Task<bool> CyclicMeasurement()
    {
        //Уведомляем что начался выход на режим 
        TestRun = TypeOfTestRun.WaitHeatPlate;

        //TODO раскомменить когда добавится нагреватель
        //вытаскиваем из списка приборов - прибор нужного типа нагреватель плиты
        HeatStand = Devices.GetTypeDevice<Heat>();
        //статус проверяемого прибора для вьюмодели
        TestCurrentDevice = HeatStand;

        //если выход нагреватель плиты выкл
        await OutputDevice(HeatStand, GetParameterForDevice().HeatValues, false);
        if (await OutputDevice(HeatStand, GetParameterForDevice().HeatValues))
        {
            //TODO раскомменить когда добавится нагреватель

            PercentCurrentTest = 40;
            //
            //вытаскиваем из списка приборов - прибор нужного типа блок питания
            //TODO раскомменить когда добавится нагреватель
        }


        TestRun = TypeOfTestRun.None;
        //Уведомляем что начался выход на режим 
        TestRun = TypeOfTestRun.CyclicMeasurement;

        isTestRun = true;
        int i = 0;
        while (isTestRun)
        {
            i++;
            //
            //TestCurrentDevice = BigLoadStand;
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
            //TestCurrentDevice = BigLoadStand;
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

    #endregion
}

public class TempChecks
{
    private List<bool> list = new();

    public void Add(bool value)
    {
        list.Add(value);
    }

    public bool IsOk => list.TrueForAll(e => e);

    public static TempChecks Start() => new TempChecks();

    protected TempChecks()
    {
    }
}