using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SST_WPF_Test_1;

public class ViewModel : Notify
{
    private Stand StandTest = new ();

    private ObservableCollection<BaseDevice> devices = new();
    private ObservableCollection<Vip> vips = new();

    public ObservableCollection<BaseDevice> Devices
    {
        get => devices;
        set => Set(ref devices, value);
    }
    public ObservableCollection<Vip> Vips
    {
        get => vips;
        set => Set(ref vips, value);
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
        foreach (var vip in StandTest.VipsStand)
        {
            vips.Add(vip);
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
        
    }


    #region Команды

    #region Команды Общие

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
        return true;
    }

    /// <summary>
    /// Команда ПРОДОЛЖИТЬ/ДАЛЕЕ
    /// </summary>
    public ICommand NextCmd { get; }

    Task OnNextCmdExecuted(object p)
    {
        PrimaryCheckDevicesTab = true;
        //обработчик команды
        return Task.CompletedTask;
    }

    bool CanNextCmdExecuted(object p)
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
        await StandTest.PrimaryCheckDevices();
    }

    bool CanStartTestDevicesCmdExecuted(object p)
    {
        if (TestRun == TypeOfTestRun.PrimaryCheckDevices)
        {
            return false;
        }
        return true;
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
        return true;
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
        return true;
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
            }

            if (testRun == TypeOfTestRun.PrimaryCheckDevices)
            {
                TextCurrentTest = " Идет проверка внешних устройств";
            }

            if (testRun == TypeOfTestRun.PrimaryCheckDevicesReady)
            {
                TextCurrentTest = " Проверка внешних устройств завершена";
            }

            if (testRun == TypeOfTestRun.MeasurementZero)
            {
                TextCurrentTest = " Идет нулевой замер";
            }

            if (testRun == TypeOfTestRun.MeasurementZeroReady)
            {
                TextCurrentTest = " Нулевой замер завершен";
            }

            if (testRun == TypeOfTestRun.WaitSettingToOperatingMode)
            {
                TextCurrentTest = " Идет Выход на режим (нагрев основания)";
            }

            if (testRun == TypeOfTestRun.SettingToOperatingModeReady)
            {
                TextCurrentTest = " Выход на режим (нагрев основания) завершен";
            }

            if (testRun == TypeOfTestRun.CyclicMeasurement)
            {
                TextCurrentTest = " Идет циклический замер";
            }

            if (testRun == TypeOfTestRun.Error)
            {
                TextCurrentTest = " Ошибка";
            }
        }
    }

    private string textCurrentTest;

    /// <summary>
    /// Уведомляет текстом чей сейчас тест идет
    /// </summary>
    public string TextCurrentTest
    {
        get => textCurrentTest;
        set => Set(ref textCurrentTest, value);
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

    #endregion

    //

    #region Поля Подключение устройств

    private bool primaryCheckDevicesTab;

    /// <summary>
    /// Включатель влкадки подключения устройств
    /// </summary>
    public bool PrimaryCheckDevicesTab
    {
        //TODO уточнооить кк работает
        //get => TestRun is TypeOfTestRun.PrimaryCheckDevices or TypeOfTestRun.None;
        set => Set(ref primaryCheckDevicesTab, value);
    }

    #endregion

    #region Настройки

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