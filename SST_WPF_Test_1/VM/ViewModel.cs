using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace SST_WPF_Test_1;

public class ViewModel : Notify
{
    /// <summary>
    /// Класс библиотеки
    /// </summary>
    BaseLibCmd libCmd = BaseLibCmd.getInstance();

    private MySerializer serializer = new MySerializer();

    private Stand StandTest = new();

    private ObservableCollection<BaseDevice> devices = new();

    public ObservableCollection<BaseDevice> Devices
    {
        get => devices;
        set => Set(ref devices, value);
    }

    private ObservableCollection<Vip> allVips = new();

    public ObservableCollection<Vip> AllVips
    {
        get => allVips;
        set => Set(ref allVips, value);
    }


    public ViewModel()
    {
        //включаем уведомления из модели
        StandTest.PropertyChanged += StandTestOnPropertyChanged;
        
        //включаем кладку Подключения Устройств
        StandTest.TestRun = TypeOfTestRun.Stop;

        ConfigDevices();

        #region Команды

        StartTestDevicesCmd = new ActionCommand(OnStartTestDevicesCmdExecuted, CanStartTestDevicesCmdExecuted);
        OpenSettingsDevicesCmd = new ActionCommand(OnOpenSettingsDevicesCmdExecuted, CanOpenSettingsDevicesCmdExecuted);
        CancelAllTestCmd = new ActionCommand(OnCancelAllTestCmdExecuted, CanCancelAllTestCmdExecuted);
        NextCmd = new ActionCommand(OnNextCmdExecuted, CanNextCmdExecuted);
        CreateReportCmd = new ActionCommand(OnCreateReportCmdExecuted, CanCreateReportCmdExecuted);

        #region Настройки

        SaveSettingsCmd = new ActionCommand(OnSaveSettingsCmdExecuted, CanSaveSettingsCmdExecuted);
        SaveSettingTestCmd = new ActionCommand(OnSaveSettingTestCmdExecuted, CanSaveSettingTestCmdExecuted);

        SaveTestAllTimeCmd = new ActionCommand(OnSaveTestAllTimeCmdExecuted, CanSaveTestAllTimeCmdExecuted);
        SaveReportPlaceCmd = new ActionCommand(OnSaveReportPlaceCmdExecuted, CanSaveReportPlaceCmdExecuted);

        AddCmdFromDeviceCmd = new ActionCommand(OnAddCmdFromDeviceCmdExecuted, CanAddCmdFromDeviceCmdExecuted);
        RemoveCmdFromDeviceCmd = new ActionCommand(OnRemoveCmdFromDeviceCmdExecuted, CanRemoveCmdFromDeviceCmdExecuted);

        #endregion

        #endregion
    }
    
    
    
    public void ConfigDevices()
    {
        var deserializeLib = serializer.DeserializeLib();
        libCmd.DeviceCommands = deserializeLib;
        var deserializeDevices = serializer.DeserializeDevices();

        foreach (var device in deserializeDevices)
        {
            devices.Add(device);
            StandTest.Devices.Add(device);
        }
        // devices.Add(StandTest.MultimeterStand);
        // devices.Add(StandTest.SupplyStand);
        // devices.Add(StandTest.ThermometerStand);
        // devices.Add(StandTest.SmallLoadStand);
        // devices.Add(StandTest.BigLoadStand);
        // devices.Add(StandTest.HeatStand);
        //
        // foreach (var device in StandTest.SwitchersMetersStand)
        // {
        //     devices.Add(device);
        // }

        foreach (var vip in StandTest.VipsStand)
        {
            allVips.Add(vip);
        }
        
    }
    
    
    private void StandTestOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        #region Статусы/проценты - теста/устройств

        if (e.PropertyName == nameof(TestRun))
        {
            TestRun = StandTest.TestRun;
        }

        if (e.PropertyName == nameof(PercentCurrentTest))
        {
            PercentCurrentTest = StandTest.PercentCurrentTest;
        }

        if (e.PropertyName == "TestCurrentDevice")
        {
            TextCurrentTestDevice = StandTest.TestCurrentDevice.IsDeviceType;
        }

        #endregion

        #region Время теста

        if (e.PropertyName == nameof(TestAllTime))
        {
            TestAllTime = StandTest.TestAllTime;
        }

        if (e.PropertyName == nameof(TestIntervalTime))
        {
            TestIntervalTime = StandTest.TestIntervalTime;
        }

        if (e.PropertyName == nameof(TestLeftEndTime))
        {
            TestLeftEndTime = StandTest.TestLeftEndTime;
        }

        #endregion
    }


    #region Команды

    #region Команды Общие

    /// <summary>
    /// Команда ПРОДОЛЖИТЬ/ДАЛЕЕ
    /// </summary>
    public ICommand NextCmd { get; }

    async Task OnNextCmdExecuted(object p)
    {
        if (TestRun == TypeOfTestRun.PrimaryCheckDevicesReady)
        {
            SelectTab = 1;
        }
        else if (TestRun == TypeOfTestRun.PrimaryCheckVipsReady)
        {
            SelectTab = 2;
        }
    }

    bool CanNextCmdExecuted(object p)
    {
        return TestRun == TypeOfTestRun.PrimaryCheckDevicesReady ||
               TestRun == TypeOfTestRun.PrimaryCheckVipsReady;
    }

    /// <summary>
    /// Команда ОТМЕНИТЬ испытания
    /// </summary>
    public ICommand CancelAllTestCmd { get; }

    Task OnCancelAllTestCmdExecuted(object p)
    {
        //TODO добавить canellded
        StandTest.ResetCurrentTest();
        SelectTab = 0;
        return Task.CompletedTask;
    }

    bool CanCancelAllTestCmdExecuted(object p)
    {
        return true;
    }

    #endregion

    #region Команды Подключение устройств

    /// <summary>
    /// Команда ЗАПУСТИТЬ исптания
    /// </summary>
    public ICommand StartTestDevicesCmd { get; }

    async Task OnStartTestDevicesCmdExecuted(object p)
    {
        if (SelectTab == 0)
        {
            await StandTest.PrimaryCheckDevices();
        }

        else if (SelectTab == 1)
        {
            await StandTest.PrimaryCheckVips();
        }

        else if (SelectTab == 2)
        {
            var mesZero = await StandTest.MeasurementZero();

            if (mesZero)
            {
                await StandTest.CyclicMeasurement();
            }
        }
    }

    bool CanStartTestDevicesCmdExecuted(object p)
    {
        return TestRun != TypeOfTestRun.PrimaryCheckDevices || TestRun != TypeOfTestRun.PrimaryCheckVips ||
               TestRun != TypeOfTestRun.MeasurementZero;
    }

    /// <summary>
    /// Команда открыть ФАЙЛ КОНФИГУРАЦИИ/НАСТРОЙКУ внешних устройств
    /// </summary>
    public ICommand OpenSettingsDevicesCmd { get; }

    Task OnOpenSettingsDevicesCmdExecuted(object p)
    {
        //обработчик команды
        return Task.CompletedTask;
    }

    bool CanOpenSettingsDevicesCmdExecuted(object p)
    {
        return TestRun switch
        {
            TypeOfTestRun.PrimaryCheckDevices => false,
            TypeOfTestRun.PrimaryCheckVips => false,
            TypeOfTestRun.DeviceOperation => false,
            TypeOfTestRun.MeasurementZero => false,
            _ => true
        };
    }

    #endregion

    //

    #region Команды подключения Випов

    public ICommand CreateReportCmd { get; }

    Task OnCreateReportCmdExecuted(object p)
    {
        //обработчик команды
        StandTest.SaveReportPlace();
        return Task.CompletedTask;
    }

    bool CanCreateReportCmdExecuted(object p)
    {
        return true;
    }

    #endregion

    #region Команды Настроек

    /// <summary>
    /// Команда открыть ФАЙЛ КОНФИГУРАЦИИ/НАСТРОЙКУ внешних устройств
    /// </summary>
    public ICommand SaveSettingsCmd { get; }

    Task OnSaveSettingsCmdExecuted(object p)
    {
        var index = Devices.IndexOf(selectDevice);
        Devices[index].Name = NameDevice;
        Devices[index].Config.PortName = PortName;
        Devices[index].Config.Baud = Baud;
        Devices[index].Config.StopBits = StopBits;
        Devices[index].Config.Parity = Parity;
        Devices[index].Config.DataBits = DataBits;
        Devices[index].Config.Dtr = Dtr;

        NameDevice = selectDevice.Name;
        PortName = selectDevice.GetConfigDevice().PortName;
        Parity = selectDevice.GetConfigDevice().Baud;
        StopBits = selectDevice.GetConfigDevice().StopBits;
        Parity = selectDevice.GetConfigDevice().Parity;
        DataBits = selectDevice.GetConfigDevice().DataBits;
        Dtr = selectDevice.GetConfigDevice().Dtr;

        if (selectDevice is SwitcherMeter)
        {
            StandTest.MultiSetConfigSwitcher(TypePort.SerialInput, PortName, Baud, StopBits, Parity,
                DataBits, Dtr);
            return Task.CompletedTask;
        }

        Devices[index].SetConfigDevice(TypePort.SerialInput, PortName, Baud, StopBits, Parity,
            DataBits, Dtr);
        var temp = Devices.ToList();
        serializer.SerializeDevices(temp);
        StandTest.Devices = Devices;
       
        selectedDeviceCommand.Source =
            SelectDevice?.LibCmd.DeviceCommands.Where(x =>
                x.Key.NameDevice == selectDevice.Name);
        OnPropertyChanged(nameof(SelectedDeviceCommand));
        
        return Task.CompletedTask;
    }

    bool CanSaveSettingsCmdExecuted(object p)
    {
        return true;
    }


    public ICommand SaveSettingTestCmd { get; }

    Task OnSaveSettingTestCmdExecuted(object p)
    {
        //обработчик команды
        return Task.CompletedTask;
    }

    bool CanSaveSettingTestCmdExecuted(object p)
    {
        return true;
    }


    public ICommand SaveReportPlaceCmd { get; }

    Task OnSaveReportPlaceCmdExecuted(object p)
    {
        //обработчик команды

        StandTest.SaveReportPlace();


        return Task.CompletedTask;
    }

    bool CanSaveReportPlaceCmdExecuted(object p)
    {
        return true;
    }

    public ICommand SaveTestAllTimeCmd { get; }

    Task OnSaveTestAllTimeCmdExecuted(object p)
    {
        //обработчик команды
        StandTest.SetTimesTest(TestAllTime, TestIntervalTime);
        return Task.CompletedTask;
    }

    bool CanSaveTestAllTimeCmdExecuted(object p)
    {
        return true;
    }

    public ICommand AddCmdFromDeviceCmd { get; }

    Task OnAddCmdFromDeviceCmdExecuted(object p)
    {
        libCmd.AddCommand(nameCmdLib, SelectDevice.Name, transmitCmdLib, receiveCmdLib, delayCmdLib,
            startStingCmdLib, endStringCmdLib, pingCountCmdLib, TypeMessageCmdLib);

        selectedDeviceCommand.Source =
            SelectDevice?.LibCmd.DeviceCommands.Where(x =>
                x.Key.NameDevice == selectDevice.Name);
        OnPropertyChanged(nameof(SelectedDeviceCommand));

        serializer.LibCmd = libCmd.DeviceCommands;
        serializer.SerializeLib();


        return Task.CompletedTask;
    }

    bool CanAddCmdFromDeviceCmdExecuted(object p)
    {
        return true;
    }

    public ICommand RemoveCmdFromDeviceCmd { get; }

    Task OnRemoveCmdFromDeviceCmdExecuted(object p)
    {
        libCmd.DeleteCommand(selectedCmdLib.Key.NameCmd, selectedCmdLib.Key.NameDevice);
        selectedDeviceCommand.Source =
            SelectDevice?.LibCmd.DeviceCommands.Where(x =>
                x.Key.NameDevice == selectDevice.Name);
        OnPropertyChanged(nameof(SelectedDeviceCommand));

        serializer.LibCmd = libCmd.DeviceCommands;
        serializer.SerializeLib();
        return Task.CompletedTask;
    }

    bool CanRemoveCmdFromDeviceCmdExecuted(object p)
    {
        return true;
    }

    #endregion

    #endregion


    #region Поля

    #region Общие

    void TabsDisable()
    {
        PrimaryCheckDevicesTab = false;
        PrimaryCheckVipsTab = false;
        CheckVipsTab = false;
        SettingsTab = false;
    }

    void TabsEnable()
    {
        PrimaryCheckDevicesTab = true;
        PrimaryCheckVipsTab = true;
        CheckVipsTab = true;
        SettingsTab = true;
    }

    private TypeOfTestRun testRun;

    /// <summary>
    /// Уведомляет чей сейчас тест идет
    /// </summary>
    public TypeOfTestRun TestRun
    {
        get => testRun;
        set
        {
            if (!Set(ref testRun, value)) return;

            if (testRun == TypeOfTestRun.Stop)
            {
                TextCurrentTest = "";
                PercentCurrentTest = 0;

                TabsDisable();
                PrimaryCheckDevicesTab = true;
            }


            if (testRun == TypeOfTestRun.None)
            {
                TextCurrentTest = "";
                PercentCurrentTest = 0;

                PrimaryCheckDevicesTab = true;
                PrimaryCheckVipsTab = true;
                CheckVipsTab = true;
                SettingsTab = true;
            }

            //
            if (testRun == TypeOfTestRun.PrimaryCheckDevices)
            {
                TextCurrentTest = " Предпроверка устройств";
                TabsDisable();
                PrimaryCheckDevicesTab = true;
            }

            if (testRun == TypeOfTestRun.PrimaryCheckDevicesReady)
            {
                TextCurrentTest = " Предпроверка устройств ОК";
                PrimaryCheckDevicesTab = true;
                PrimaryCheckVipsTab = true;
            }

            //
            if (testRun == TypeOfTestRun.PrimaryCheckVips)
            {
                TextCurrentTest = " Предпроверка Випов";

                TabsDisable();
                PrimaryCheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.PrimaryCheckVipsReady)
            {
                TextCurrentTest = " Предпроверка Випов Ок";
                PrimaryCheckDevicesTab = true;
                CheckVipsTab = true;
            }

            //
            if (testRun == TypeOfTestRun.DeviceOperation)
            {
                TextCurrentTest = $" Включение устройства";
                TabsDisable();
                CheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.DeviceOperationReady)
            {
                TextCurrentTest = " Включение устройства Ок";
                TabsEnable();
            }
            //

            if (testRun == TypeOfTestRun.MeasurementZero)
            {
                TextCurrentTest = " Нулевой замер";
                TabsDisable();
                CheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.MeasurementZeroReady)
            {
                TextCurrentTest = " Нулевой замер ОК";
                TabsEnable();
            }

            if (testRun == TypeOfTestRun.WaitSettingToOperatingMode)
            {
                TextCurrentTest = " Нагрев основания";
                TabsDisable();
                CheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.WaitSettingToOperatingModeReady)
            {
                TextCurrentTest = " Нагрев основания ОК";
                TabsEnable();
            }

            if (testRun == TypeOfTestRun.CyclicMeasurement)
            {
                TextCurrentTest = " Циклический замер";
                TabsDisable();
                CheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.CycleWait)
            {
                TextCurrentTest = " Ожидание замерф";
                TabsDisable();
                CheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.CyclicMeasurementReady)
            {
                TextCurrentTest = " Циклический замеы закончены";
                TabsDisable();
                CheckVipsTab = true;
            }

            if (testRun == TypeOfTestRun.Error)
            {
                TextCurrentTest = " Ошибка!";
            }
        }
    }

    private string textCurrentTest;

    /// <summary>
    /// Уведомляет текстом этап тестов
    /// </summary>
    public string TextCurrentTest
    {
        get => textCurrentTest;
        set => Set(ref textCurrentTest, value);
    }

    private string textCurrentTestDevice;

    /// <summary>
    /// Уведомляет текстом этап тестов
    /// </summary>
    public string TextCurrentTestDevice
    {
        get => textCurrentTestDevice;
        set => Set(ref textCurrentTestDevice, value);
    }


    private double percentCurrentTest;

    /// <summary>
    /// Уведомляет сколько процентов текущего теста прошло
    /// </summary>
    public double PercentCurrentTest
    {
        get => percentCurrentTest;

        set => Set(ref percentCurrentTest, value);
    }

    private double selectTab;

    /// <summary>
    /// Уведомляет сколько процентов текущего теста прошло
    /// </summary>
    public double SelectTab
    {
        get => selectTab;

        set => Set(ref selectTab, value);
    }

    #endregion

    #region Поля Подключение устройств

    private bool primaryCheckDevicesTab;

    /// <summary>
    /// Включатель влкадки подключения устройств 0
    /// </summary>
    public bool PrimaryCheckDevicesTab
    {
        //TODO уточнооить кк работает
        get => primaryCheckDevicesTab;
        set => Set(ref primaryCheckDevicesTab, value);
    }


    private bool primaryCheckVipsTab;

    /// <summary>
    /// Включатель влкадки предварительной проверки випов 1
    /// </summary>
    public bool PrimaryCheckVipsTab
    {
        //TODO уточнооить кк работает
        get => primaryCheckVipsTab;
        set => Set(ref primaryCheckVipsTab, value);
    }


    private bool checkVipsTab;

    /// <summary>
    /// Включатель влкадки проверки випов 2
    /// </summary>
    public bool CheckVipsTab
    {
        //TODO уточнооить кк работает
        get => checkVipsTab;
        set => Set(ref checkVipsTab, value);
    }

    private bool settingsTab;

    /// <summary>
    /// Включатель влкадки  настроек 3
    /// </summary>
    public bool SettingsTab
    {
        //TODO уточнооить кк работает
        get => settingsTab;
        set => Set(ref settingsTab, value);
    }

    #endregion

    #region Поля подключение ВИПов

    #endregion

    #region Поля Настройки

    private bool enabledDeviceName;

    public bool EnabledDeviceName
    {
        get => enabledDeviceName;
        set { Set(ref enabledDeviceName, value); }
    }


    private BaseDevice selectDevice;

    /// <summary>
    /// Выбор устройства в насьтройках
    /// </summary>
    public BaseDevice SelectDevice
    {
        get { return selectDevice; }
        set
        {
            if (!Set(ref selectDevice, value)) return;

            NameDevice = selectDevice.Name;
            PortName = selectDevice.GetConfigDevice().PortName;
            Baud = selectDevice.GetConfigDevice().Baud;
            StopBits = selectDevice.GetConfigDevice().StopBits;
            Parity = selectDevice.GetConfigDevice().Parity;
            DataBits = selectDevice.GetConfigDevice().DataBits;
            Dtr = selectDevice.GetConfigDevice().Dtr;
            //

            if (selectDevice is SwitcherMeter)
            {
                EnabledDeviceName = false;
            }

            if (selectDevice is not SwitcherMeter)
            {
                EnabledDeviceName = true;
            }

            selectedDeviceCommand.Source =
                value?.LibCmd.DeviceCommands.Where(x =>
                    x.Key.NameDevice == selectDevice.Name);

            OnPropertyChanged(nameof(SelectedDeviceCommand));
        }
    }

    private readonly CollectionViewSource selectedDeviceCommand = new();
    public ICollectionView? SelectedDeviceCommand => selectedDeviceCommand?.View;


    private string nameDevice;

    /// <summary>
    ///
    /// </summary>
    public string NameDevice
    {
        get => nameDevice;
        set
        {
            Set(ref nameDevice, value);
        }
    }


    private string portName;

    /// <summary>
    ///
    /// </summary>
    public string PortName
    {
        get => portName;
        set => Set(ref portName, value);
    }

    private int baud;

    /// <summary>
    /// 
    /// </summary>
    public int Baud
    {
        get => baud;
        set => Set(ref baud, value);
    }

    private int stopBits;

    public int StopBits
    {
        get => stopBits;
        set => Set(ref stopBits, value);
    }

    private int parity;

    public int Parity
    {
        get => parity;
        set => Set(ref parity, value);
    }

    private int dataBits;

    public int DataBits
    {
        get => dataBits;
        set => Set(ref dataBits, value);
    }

    private bool dtr;

    public bool Dtr
    {
        get => dtr;
        set => Set(ref dtr, value);
    }

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


    #region Добавление команды

    private string nameCmdLib;

    public string NameCmdLib
    {
        get => nameCmdLib;
        set => Set(ref nameCmdLib, value);
    }

    private string transmitCmdLib;

    public string TransmitCmdLib
    {
        get => transmitCmdLib;
        set => Set(ref transmitCmdLib, value);
    }

    private string receiveCmdLib;

    public string ReceiveCmdLib
    {
        get => receiveCmdLib;
        set => Set(ref receiveCmdLib, value);
    }

    private string terminatorCmdLib;

    public string TerminatorCmdLib
    {
        get => terminatorCmdLib;
        set => Set(ref terminatorCmdLib, value);
    }

    private TypeCmd typeMessageCmdLib;

    public TypeCmd TypeMessageCmdLib
    {
        get => typeMessageCmdLib;
        set => Set(ref typeMessageCmdLib, value);
    }

    private int delayCmdLib;

    public int DelayCmdLib
    {
        get => delayCmdLib;
        set => Set(ref delayCmdLib, value);
    }

    private int pingCountCmdLib;

    public int PingCountCmdLib
    {
        get => pingCountCmdLib;
        set => Set(ref pingCountCmdLib, value);
    }

    private string startStingCmdLib;

    public string StartStingCmdLib
    {
        get => startStingCmdLib;
        set => Set(ref startStingCmdLib, value);
    }

    private string endStringCmdLib;

    public string EndStringCmdLib
    {
        get => endStringCmdLib;
        set => Set(ref endStringCmdLib, value);
    }

    private KeyValuePair<DeviceIdentCmd, DeviceCmd> selectedCmdLib;

    public KeyValuePair<DeviceIdentCmd, DeviceCmd> SelectedCmdLib
    {
        get => selectedCmdLib;
        set
        {
            selectedCmdLib = value;
            OnPropertyChanged(nameof(SelectedCmdLib));
        }
    }

    #endregion

    #endregion

    #endregion
}