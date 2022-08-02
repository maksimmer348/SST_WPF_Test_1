using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SST_WPF_Test_1;

public class ViewModel : Notify
{
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
        StandTest.PropertyChanged += StandTestOnPropertyChanged;
        //включаем кладку Подключения Устройств
        PrimaryCheckDevicesTab = true;
        //TODO остальные выключаем
        //ВключитьНастройкуВиповТаб = false;

        devices.Add(StandTest.MultimeterStand);
        devices.Add(StandTest.SupplyStand);
        devices.Add(StandTest.SupplyStand);
        devices.Add(StandTest.ThermometerStand);
        devices.Add(StandTest.BigLoadStand);
        devices.Add(StandTest.SmallLoadStand);
        devices.Add(StandTest.HeatStand);
        foreach (var device in StandTest.SwitchersMetersStand)
        {
            devices.Add(device);
        }

        foreach (var vip in StandTest.VipsStand)
        {
            allVips.Add(vip);
        }


        #region Команды

        StartTestDevicesCmd = new ActionCommand(OnStartTestDevicesCmdExecuted, CanStartTestDevicesCmdExecuted);
        RepeatTestDevicesCmd = new ActionCommand(OnRepeatTestDevicesCmdExecuted, CanRepeatTestDevicesCmdExecuted);
        OpenSettingsDevicesCmd = new ActionCommand(OnOpenSettingsDevicesCmdExecuted, CanOpenSettingsDevicesCmdExecuted);
        CancelAllTestCmd = new ActionCommand(OnCancelAllTestCmdExecuted, CanCancelAllTestCmdExecuted);
        NextCmd = new ActionCommand(OnNextCmdExecuted, CanNextCmdExecuted);

        SaveSettingsCmd = new ActionCommand(OnSaveSettingsCmdExecuted, CanSaveSettingsCmdExecuted);

        #endregion
    }

    private void StandTestOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "TestRun")
        {
            TestRun = StandTest.TestRun;
        }

        if (e.PropertyName == "PercentCurrentTest")
        {
            PercentCurrentTest = StandTest.PercentCurrentTest;
        }

        if (e.PropertyName == "TestCurrentDevice")
        {
            TextCurrentTestDevice = StandTest.TestCurrentDevice.IsDeviceType;
        }
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
            TestRun = TypeOfTestRun.None;
            SelectTab = 1;
        }
        else if (TestRun == TypeOfTestRun.PrimaryCheckVipsReady)
        {
            SelectTab = 2;
            await StandTest.MeasurementZero();
        }
        //обработчик команды
    }

    /// <summary>
    /// Команда ОТМЕНИТЬ испытания
    /// </summary>
    public ICommand CancelAllTestCmd { get; }

    Task OnCancelAllTestCmdExecuted(object p)
    {
        PrimaryCheckDevicesTab = false;
        //обработчик команды

        return Task.CompletedTask;
    }

    bool CanCancelAllTestCmdExecuted(object p)
    {
        return TestRun == TypeOfTestRun.PrimaryCheckDevices || TestRun == TypeOfTestRun.PrimaryCheckVips;
    }


    bool CanNextCmdExecuted(object p)
    {
        return TestRun == TypeOfTestRun.PrimaryCheckDevicesReady || TestRun == TypeOfTestRun.PrimaryCheckVipsReady;
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
    }

    bool CanStartTestDevicesCmdExecuted(object p)
    {
        return TestRun != TypeOfTestRun.PrimaryCheckDevices || TestRun != TypeOfTestRun.PrimaryCheckVips;
    }

    /// <summary>
    /// Команда ПОВТОРИТЬ проверку устройств
    /// </summary>
    public ICommand RepeatTestDevicesCmd { get; }

    Task OnRepeatTestDevicesCmdExecuted(object p)
    {
        //обработчик команды
        return Task.CompletedTask;
    }

    bool CanRepeatTestDevicesCmdExecuted(object p)
    {
        return TestRun != TypeOfTestRun.PrimaryCheckDevices || TestRun != TypeOfTestRun.PrimaryCheckVips;
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
        return TestRun != TypeOfTestRun.PrimaryCheckDevices || TestRun != TypeOfTestRun.PrimaryCheckVips;
    }

    #endregion

    //

    #region Команды подключения Випов

    #endregion

    #region Команды Настроек

    /// <summary>
    /// Команда открыть ФАЙЛ КОНФИГУРАЦИИ/НАСТРОЙКУ внешних устройств
    /// </summary>
    public ICommand SaveSettingsCmd { get; }

    Task OnSaveSettingsCmdExecuted(object p)
    {
        //обработчик команды
        var index = Devices.IndexOf(selectDevice);
        Devices[index].Config.PortName = portName;
        Devices[index].Config.Baud = baud;
        StopBits = selectDevice.GetConfigDevice().StopBits;
        Parity = selectDevice.GetConfigDevice().Parity;
        DataBits = selectDevice.GetConfigDevice().DataBits;
        Dtr = selectDevice.GetConfigDevice().Dtr;

        Devices[index].SetConfigDevice(TypePort.SerialInput, PortName, Baud, StopBits, Parity, DataBits, Dtr);
        return Task.CompletedTask;
    }

    bool CanSaveSettingsCmdExecuted(object p)
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
                TabsEnable();
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
                TabsEnable();
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

            PortName = selectDevice.GetConfigDevice().PortName;
            Baud = selectDevice.GetConfigDevice().Baud;
            StopBits = selectDevice.GetConfigDevice().StopBits;
            Parity = selectDevice.GetConfigDevice().Parity;
            DataBits = selectDevice.GetConfigDevice().DataBits;
            Dtr = selectDevice.GetConfigDevice().Dtr;
            //
            selectedDeviceCommand.Source = value?.LibCmd.DeviceCommands;
            OnPropertyChanged(nameof(SelectedDeviceCommand));
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

    //с помощью этого эл можно выводить в представление отфильтроване эл
    private readonly CollectionViewSource selectedDeviceCommand = new();

    /// <summary>
    /// Выбраные студенты из выбранной группы
    /// </summary>
    public ICollectionView SelectedDeviceCommand => selectedDeviceCommand?.View;

    #endregion

    #endregion
}