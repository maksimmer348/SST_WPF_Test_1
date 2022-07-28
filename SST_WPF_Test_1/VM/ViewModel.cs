using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SST_WPF_Test_1;

public class ViewModel : Notify
{
    private Stand StandTest = new ();

    private ObservableCollection<BaseDevice> devices = new();

    public ObservableCollection<BaseDevice> Devices
    {
        get => devices;
        set => Set(ref devices, value);
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

        if (e.PropertyName == "StatusTestColor")
        {
          
        }
    }


    #region Команды

    #region Команды Общие

    /// <summary>
    /// Команда ОТМЕНИТЬ испытания
    /// </summary>
    public ICommand CancelAllTestCmd { get; }

    void OnCancelAllTestCmdExecuted(object p)
    {
        PrimaryCheckDevicesTab = false;
        //обработчик команды
    }

    bool CanCancelAllTestCmdExecuted(object p)
    {
        return true;
    }

    /// <summary>
    /// Команда ПРОДОЛЖИТЬ/ДАЛЕЕ
    /// </summary>
    public ICommand NextCmd { get; }

    void OnNextCmdExecuted(object p)
    {
        PrimaryCheckDevicesTab = true;
        //обработчик команды
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

    async void OnStartTestDevicesCmdExecuted(object p)
    {
        await StandTest.PrimaryCheckDevices();
    }

    bool CanStartTestDevicesCmdExecuted(object p)
    {
        return true;
    }

    /// <summary>
    /// Команда ПОВТОРИТЬ проверку устройств
    /// </summary>
    public ICommand RepeatTestDevicesCmd { get; }

    void OnRepeatTestDevicesCmdExecuted(object p)
    {
        //обработчик команды
    }

    bool CanRepeatTestDevicesCmdExecuted(object p)
    {
        return true;
    }

    /// <summary>
    /// Команда открыть ФАЙЛ КОНФИГУРАЦИИ/НАСТРОЙКУ внешних устройств
    /// </summary>
    public ICommand OpenSettingsDevicesCmd { get; }

    void OnOpenSettingsDevicesCmdExecuted(object p)
    {
        //обработчик команды
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

    void OnSaveSettingsCmdExecuted(object p)
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

    private Brush indMultimeter;

    /// <summary>
    /// Индикатор мультиметр
    /// </summary>
    public Brush IndMultimeter
    {
        get => indMultimeter;
        set => Set(ref indMultimeter, value);
    }

    private Brush indThermometer;

    /// <summary>
    /// Индикатор термометр
    /// </summary>
    public Brush IndThermometer
    {
        get => indThermometer;
        set => Set(ref indThermometer, value);
    }

    private Brush indSupply;

    /// <summary>
    /// Индикатор БП
    /// </summary>
    public Brush IndSupply
    {
        get => indSupply;
        set => Set(ref indSupply, value);
    }

    private Brush indHeat;

    /// <summary>
    /// Индикатор Нагрев
    /// </summary>
    public Brush IndHeat
    {
        get => indHeat;
        set => Set(ref indHeat, value);
    }

    private Brush indLoadSmall;

    /// <summary>
    /// Индикатор Нагрузка Малая
    /// </summary>
    public Brush IndLoadSmall
    {
        get => indLoadSmall;
        set => Set(ref indLoadSmall, value);
    }

    private Brush indLoadBig;

    /// <summary>
    /// Индикатор Нагрузка Большая/Генератор
    /// </summary>
    public Brush IndLoadBig
    {
        get => indLoadBig;
        set => Set(ref indLoadBig, value);
    }

    private Brush indSwitch1;

    /// <summary>
    /// Индикатор 1
    /// </summary>
    public Brush IndSwitch1
    {
        get => indSwitch1;
        set => Set(ref indSwitch1, value);
    }

    private Brush indSwitch2;

    /// <summary>
    /// Индикатор 2
    /// </summary>
    public Brush IndSwitch2
    {
        get => indSwitch2;
        set => Set(ref indSwitch2, value);
    }

    private Brush indSwitch3;

    /// <summary>
    /// Индикатор 3
    /// </summary>
    public Brush IndSwitch3
    {
        get => indSwitch3;
        set => Set(ref indSwitch3, value);
    }

    private Brush indSwitch4;

    /// <summary>
    /// Индикатор 4
    /// </summary>
    public Brush IndSwitch4
    {
        get => indSwitch4;
        set => Set(ref indSwitch4, value);
    }

    private Brush indSwitch5;

    /// <summary>
    /// Индикатор 5
    /// </summary>
    public Brush IndSwitch5
    {
        get => indSwitch5;
        set => Set(ref indSwitch5, value);
    }

    private Brush indSwitch6;

    /// <summary>
    /// Индикатор 6
    /// </summary>
    public Brush IndSwitch6
    {
        get => indSwitch6;
        set => Set(ref indSwitch6, value);
    }

    private Brush indSwitch7;

    /// <summary>
    /// Индикатор 7
    /// </summary>
    public Brush IndSwitch7
    {
        get => indSwitch7;
        set => Set(ref indSwitch7, value);
    }

    private Brush indSwitch8;

    /// <summary>
    /// Индикатор 8
    /// </summary>
    public Brush IndSwitch8
    {
        get => indSwitch8;
        set => Set(ref indSwitch8, value);
    }

    private Brush indSwitch9;

    /// <summary>
    /// Индикатор 9
    /// </summary>
    public Brush IndSwitch9
    {
        get => indSwitch9;
        set => Set(ref indSwitch9, value);
    }

    private Brush indSwitch10;

    /// <summary>
    /// Индикатор 10
    /// </summary>
    public Brush IndSwitch10
    {
        get => indSwitch10;
        set => Set(ref indSwitch10, value);
    }

    private Brush indSwitch11;

    /// <summary>
    /// Индикатор 11
    /// </summary>
    public Brush IndSwitch11
    {
        get => indSwitch11;
        set => Set(ref indSwitch11, value);
    }

    private Brush ind12;

    /// <summary>
    /// Индикатор 12
    /// </summary>
    public Brush Ind12
    {
        get => ind12;
        set => Set(ref ind12, value);
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