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

    public VoltageCurrentMeter VoltmeterStand { get; set; }
    public Thermometer ThermometerStand { get; set; }
    public Supply SupplyStand { get; set; }
    public SmallLoad SmallLoadStand { get; set; }
    public BigLoad BigLoadStand { get; set; }
    public Heat HeatStand { get; set; }

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
    //CancellationTokenSource cts2 = new();

    /// <summary>
    /// Подпись на события из устройств
    /// </summary>
    public void InvokeDevices()
    {
        foreach (var device in Devices)
        {
            device.ConnectPort += OnCheckConnectPort;
            device.ConnectDevice += OnCheckDevice;
            device.Receive += Receive;
        }
        foreach (var relayVip in RelaysVips)
        {
            relayVip.ConnectPort += OnCheckConnectPort;
            relayVip.ConnectDevice += OnCheckDevice;
            relayVip.Receive += Receive;
        }
    }

    /// <summary>
    /// Установка приборов по умолчанию (для тестов)
    /// </summary>
    public void SetDevices()
    {
        if (true) //TODO (true) - если сеарилизатор недосутпен выводим исключение и создаем приборы со станрдартными настройками
        {
            VoltmeterStand = new("GDM-78255A") { RowIndex = 0, ColumnIndex = 0 };
            VoltmeterStand.SetConfigDevice(TypePort.SerialInput, "COM8", 115200, 1, 0, 8);
            VoltmeterStand.ConnectPort += OnCheckConnectPort;
            VoltmeterStand.ConnectDevice += OnCheckDevice;
            VoltmeterStand.Receive += Receive;
            Devices.Add(VoltmeterStand);


            ThermometerStand = new("GDM-78255A") { RowIndex = 0, ColumnIndex = 2 };
            ThermometerStand.SetConfigDevice(TypePort.SerialInput, "COM7", 115200, 1, 0, 8);
            ThermometerStand.ConnectPort += OnCheckConnectPort;
            ThermometerStand.ConnectDevice += OnCheckDevice;
            ThermometerStand.Receive += Receive;
            Devices.Add(ThermometerStand);

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
        //предустановленные типы випов

        ConfigVip.PrepareAddTypeVips();
        TypeVips = ConfigVip.TypeVips;
    }

    public void AddRelayToVip()
    {
        for (var index = 0; index < VipsPrepareStand.Count; index++)
        {
            var vip = VipsPrepareStand[index];
            vip.Relay = RelaysVips[index] as RelayVip;
        }
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
        foreach (var switcherMeter in Devices)
        {
            if (switcherMeter is SwitcherMeter)
            {
                switcherMeter.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
            }
        }
    }

    public void MultiSetConfigRelayVip(TypePort typePort, string portName, int baud, int stopBits, int parity,
        int dataBits,
        bool dtr = true)
    {
        foreach (var relay in RelaysVips)
        {
            relay.SetConfigDevice(typePort, portName, baud, stopBits, parity, dataBits);
        }
    }
    #endregion

    //

    #region Прием данных с приборов

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

        if (baseDevice is RelayVip)
        {
            PercentCurrentTest += ((1 / (float)Devices.Count) * 60);

            //сраниваем списки
            var ss = RelaysVips.Except(TempVerifiedDevices).ToList();

            if (!ss.Any())
            {
                ctsCheckDevice.Cancel();
            }
        }
    }

    /// <summary>
    /// Событие приема данных из прибора
    /// </summary>
    /// <param name="device">Прибор посылающий данные</param>
    /// <param name="receive">Данные</param>
    private void Receive(BaseDevice device, string receive)
    {
        //Обработка события примеа сообщения
    }

    #endregion

    //

    #region Инструменты проверки


    public ObservableCollection<BaseDevice> TempVerifiedDevices { get; set; } = new();

    //public async void ReconnectDevices(List<BaseDevice> devices)
    //{

    //    foreach (var device in devices)
    //    {
    //        device.Close();
    //    }
    //    await Task.Delay(TimeSpan.FromMilliseconds(100));
    //    foreach (var device in devices)
    //    {
    //        device.Open();
    //    }
    //}



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
    /// <param name="token"></param>
    /// <param name="externalDelay">Общая задержка проверки (по умолчанию 0)</param>
    /// <returns></returns>
    public async Task<List<BaseDevice>> CheckConnectDevices(List<BaseDevice> tempCheckDevices,
        CancellationToken token, int externalDelay = 0)
    {
        //временный список дефетктивынх приборов
        List<BaseDevice> tempErrorDevices = new List<BaseDevice>();
        try
        {
            foreach (var device in tempCheckDevices)
            {
                //отправляем команду проверки на устройство
                TestCurrentDevice = device;
                device.CheckedConnectDevice();
            }
            //ждем
            await Task.Delay(TimeSpan.FromMilliseconds(1000), ctsCheckDevice.Token);

            //сраниваем списки
            tempErrorDevices = GetErrorDevices(tempCheckDevices);

            Debug.WriteLine("Normal");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            //сраниваем списки
            tempErrorDevices = GetErrorDevices(tempCheckDevices);
            ctsCheckDevice = new CancellationTokenSource();
            return tempErrorDevices;
        }
        catch (Exception e)
        {
           throw new StandException( $"StandException: ошибка  {e.Message} при проверке устройств");
        }
        PercentCurrentTest = 100;
        return tempErrorDevices;
    }

    /// <summary>
    /// Получение списка сбоынх приборов
    /// </summary>
    /// <param name="checkedDevices">Временный списко устройств</param>
    /// <returns></returns>
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

    //

    #region Предварительные проверки устройств

    /// <summary>
    /// Сброс проверки устройств
    /// </summary>
    /// <returns></returns>
    public bool ResetCurrentTest()
    {
        //сброс статуса теста
        TestRun = TypeOfTestRun.Stop;
        PercentCurrentTest = 0;

        //TODO если сброс подтвержден вернем тру
        if (true)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА устройств
    /// </summary>
    /// <returns>Результат предаварительной проверки</returns>
    public async Task<bool> PrimaryCheckDevices()
    {
        //TODO  для канселивентов
        //сброс статуса теста
        TestRun = TypeOfTestRun.None;
        //установка статуса теста первичноая провека устройств
        TestRun = TypeOfTestRun.PrimaryCheckDevices;

        //TODO вынести в конфиг (возможно)
        int checkCountAll = 3;
        int checkCountCurrent = 1;

        while (true)
        {
            PercentCurrentTest = 0;
            checkCountCurrent++;

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
                        List<BaseDevice> errorDevicesList = await CheckConnectDevices(noErrorPortsList, ctsCheckDevice.Token);

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
                        PercentCurrentTest = 100;
                        TestCurrentDevice = new BaseDevice("0");

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
                    List<BaseDevice> errorDevicesList = await CheckConnectDevices(tempCheckDevices, ctsCheckDevice.Token);

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
                            PercentCurrentTest = 100;
                            TestCurrentDevice = new BaseDevice("0");
                            return false;
                        }
                        PercentCurrentTest = 100;
                        TestCurrentDevice = new BaseDevice("0");
                    }
                    else
                    {
                        foreach (var device in tempCheckDevices)
                        {
                            device.StatusTest = StatusDeviceTest.Ok;
                        }

                        PercentCurrentTest = 100;
                        TestRun = TypeOfTestRun.PrimaryCheckDevicesReady;
                        TestCurrentDevice = new BaseDevice("0");
                        return true;
                    }
                }
            }
            if (checkCountCurrent > checkCountAll)
            {
                PercentCurrentTest = 100;
                TestCurrentDevice = new BaseDevice("0");
                return false;
            }
        }
    }


    //TODO обьеденить в один метод
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

        //предварительная настройка тестрировать ли вип => если етсь имя то тестировать
        SetIsTestedVips();

        //вставка во временный список список Випов для проверки платок
        var tempCheckVips = VipsPrepareStand.Where(x => x.IsTested);
        //временный список для проверяеызх реле
        var tempRelays = SetRelayInVips(tempCheckVips);

        while (true)
        {
            //
            PercentCurrentTest = 0;
            //

            List<BaseDevice> errorRelaysList = await CheckConnectPorts(tempRelays);

            if (errorRelaysList.Any())
            {
                foreach (var relay in errorRelaysList)
                {
                    relay.StatusTest = StatusDeviceTest.Error;
                }
            }


            PercentCurrentTest = 100;

            TestRun = TypeOfTestRun.PrimaryCheckVipsReady;

            //сброс текущего проверямего устройства
            TestCurrentDevice = new BaseDevice("0");

            return true;

        }

        return false;
    }

    private List<BaseDevice> SetRelayInVips(IEnumerable<Vip> tempCheckVips)
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

    #region Замеры

    /// <summary>
    /// 0 замер
    /// </summary>
    /// <returns>Результат 0 замера</returns>
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

    /// <summary>
    /// Ожидание нагрева плиты
    /// </summary>
    /// <returns>Результат нагрева плиты в заданное время/или нет</returns>
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

    /// <summary>
    /// Цикл замеров
    /// </summary>
    /// <returns>Успешен ли цикл/не сбойнул ли какойто прибор</returns>
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

    #endregion


}