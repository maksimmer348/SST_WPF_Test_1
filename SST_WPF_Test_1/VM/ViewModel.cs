using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SST_WPF_Test_1;

public class ViewModel : Notify
{
    #region Модель

    private BaseLibCmd libCmd = BaseLibCmd.getInstance();

    private MySerializer serializer = new MySerializer();

    private Stand standTest = new();

    #region Devices Устройства

    /// <summary>
    /// Все утсройтсва
    /// </summary>
    private ObservableCollection<BaseDevice> allDevices = new();

    public ObservableCollection<BaseDevice> AllDevices

    {
        get => allDevices;
        set => Set(ref allDevices, value);
    }

    private ObservableCollection<BaseDevice> devices = new();

    /// <summary>
    /// Внешние устройства
    /// </summary>
    public ObservableCollection<BaseDevice> Devices
    {
        get => devices;
        set => Set(ref devices, value);
    }

    private ObservableCollection<BaseDevice> relaysVips = new();

    /// <summary>
    /// Реле Випов
    /// </summary>
    public ObservableCollection<BaseDevice> RelaysVips
    {
        get => relaysVips;
        set => Set(ref relaysVips, value);
    }

    #endregion

    #region Випы

    private ObservableCollection<Vip> allPrepareVips = new();

    public ObservableCollection<Vip> AllPrepareVips
    {
        get => allPrepareVips;
        set => Set(ref allPrepareVips, value);
    }

    private ObservableCollection<TypeVip> typeVip = new();

    public ObservableCollection<TypeVip> TypeVip
    {
        get => typeVip;
        set => Set(ref typeVip, value);
    }

    #endregion

    #endregion

    #region Конструктор ctor

    public ViewModel()
    {
        //включаем уведомления из модели
        standTest.PropertyChanged += StandTestOnPropertyChanged;

        //включаем кладку Подключения Устройств
        standTest.TestRun = TypeOfTestRun.Stop;

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

    #endregion

    public void ConfigDevices()
    {
        DeserializeDevicesAndLib();

        //SetDevices();
        AddPrepareVips();

        //TODO убрать это в десериализатор?
        TypeVip = standTest.TypeVips;
    }

    /// <summary>
    /// Добавление всех випов (даже неиспользуемых) в визуальный список 
    /// </summary>
    private void AddPrepareVips()
    {
        foreach (var vip in standTest.VipsPrepareStand)
        {
            AllPrepareVips.Add(vip);
        }
    }

    void DeserializeDevicesAndLib()
    {
        //десериализация библиотеки команд  
        var deserializeLib = serializer.DeserializeLib();
        libCmd.DeviceCommands = deserializeLib;

        //десериализация устройств
        var deserializeDevices = serializer.DeserializeDevices();

        //добавление в списко всех утсройств
        AllDevices = new ObservableCollection<BaseDevice>(deserializeDevices);
        
        DevicesInAllToSort();
    }

    private void DevicesInAllToSort()
    {
        var devices = AllDevices.Where(x => x is not RelayVip);
        Devices = new ObservableCollection<BaseDevice>(devices);
        standTest.Devices = Devices;

        var relays = AllDevices.Where(x => x is RelayVip);
        RelaysVips = new ObservableCollection<BaseDevice>(relays);
        standTest.RelaysVips = RelaysVips;
        
        standTest.AddRelayToVip();
        standTest.InvokeDevices();
    }

    /// <summary>
    /// Добавление Устройств в списко всех устройств для настроек
    /// </summary>
    void SetDevices()
    {
        // AllDevices.Add(standTest.VoltmeterStand);
        // AllDevices.Add(standTest.ThermometerStand);
        // AllDevices.Add(standTest.SupplyStand);
        // //TODO вернуть 
        // //devices.Add(standTest.SmallLoadStand);
        // AllDevices.Add(standTest.BigLoadStand);
        // //TODO вернуть 
        // //devices.Add(standTest.HeatStand);
        //
        // foreach (var device in )
        // {
        //     AllDevices.Add(device);
        // }

        foreach (var device in standTest.Devices)
        {
            AllDevices.Add(device);
        }

        foreach (var relay in standTest.RelaysVips)
        {
            AllDevices.Add(relay);
        }
    }

    /// <summary>
    /// Обновление статусов событие из модели
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StandTestOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        #region Статусы/проценты - теста/устройств

        if (e.PropertyName == nameof(TestRun))
        {
            TestRun = standTest.TestRun;
        }

        if (e.PropertyName == nameof(PercentCurrentTest))
        {
            PercentCurrentTest = standTest.PercentCurrentTest;
        }

        if (e.PropertyName == "TestCurrentDevice")
        {
            TextCurrentTestDevice = standTest.TestCurrentDevice.IsDeviceType;
        }

        #endregion

        #region Время теста

        if (e.PropertyName == nameof(TestAllTime))
        {
            TestAllTime = standTest.SetTestAllTime;
        }

        if (e.PropertyName == nameof(TestIntervalTime))
        {
            TestIntervalTime = standTest.SetTestIntervalTime;
        }

        if (e.PropertyName == nameof(TestLeftEndTime))
        {
            TestLeftEndTime = standTest.TestLeftEndTime;
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
            PercentCurrentTest = 0;
        }
        else if (TestRun == TypeOfTestRun.PrimaryCheckVipsReady)
        {
            SelectTab = 2;
            PercentCurrentTest = 0;
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

    async Task OnCancelAllTestCmdExecuted(object p)
    {
        //TODO добавить canellded
        await standTest.ResetCurrentTest();
        SelectTab = 0;
    }

    bool CanCancelAllTestCmdExecuted(object p)
    {
        return true;
    }

    #endregion

    //

    #region Команды Подключение устройств

    /// <summary>
    /// Команда ЗАПУСТИТЬ исптания
    /// </summary>
    public ICommand StartTestDevicesCmd { get; }

    async Task OnStartTestDevicesCmdExecuted(object p)
    {
        if (SelectTab == 0)
        {
            try
            {
                await standTest.PrimaryCheckDevices();
            }
            catch (DeviceException e)
            {
                const string caption = "Ошибка предварительной проверки устройств";
                var result = MessageBox.Show(e.Message + " Перейти в настройки?", caption, MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    SelectTab = 3;
                }
            }
        }

        else if (SelectTab == 1)
        {
            try
            {
                await standTest.PrimaryCheckVips();
            }
            catch (DeviceException e)
            {
                const string caption = "Ошибка предварительной проверки плат Випов";
                var result = MessageBox.Show(e.Message + " Перейти в настройки?", caption, MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    SelectTab = 3;
                }
            }
        }

        else if (SelectTab == 2)
        {
            try
            {
                var mesZero = await standTest.MeasurementZero();
                if (mesZero)
                {
                    var heat = await standTest.WaitForTestMode();
                    if (heat)
                    {
                        await standTest.CyclicMeasurement();
                    }
                }
            }
            catch (DeviceException e)
            {
                const string caption = "Ошибка 0 замера";
                var result = MessageBox.Show(e.Message + " Перейти в настройки?", caption, MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    SelectTab = 3;
                }
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
        //TODO отправить отсюда в настройки
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

    /// <summary>
    /// Команда СОЗДАТЬ отчет о испытаниях
    /// </summary>
    public ICommand CreateReportCmd { get; }

    Task OnCreateReportCmdExecuted(object p)
    {
        //обработчик команды
        standTest.SaveReportPlace();
        return Task.CompletedTask;
    }

    bool CanCreateReportCmdExecuted(object p)
    {
        return true;
    }

    #endregion

    //

    #region Команды Настроек

    /// <summary>
    /// Команда СОХРАНИТЬ выбранное внешнее устройство
    /// </summary>
    public ICommand SaveSettingsCmd { get; }

    Task OnSaveSettingsCmdExecuted(object p)
    {
        var index = AllDevices.IndexOf(selectDevice);
        AllDevices[index].Name = NameDevice;
        AllDevices[index].Config.PortName = PortName;
        AllDevices[index].Config.Baud = Baud;
        AllDevices[index].Config.StopBits = StopBits;
        AllDevices[index].Config.Parity = Parity;
        AllDevices[index].Config.DataBits = DataBits;
        AllDevices[index].Config.Dtr = Dtr;

        NameDevice = selectDevice.Name;

        try
        {
            PortName = selectDevice.GetConfigDevice().PortName;
            Parity = selectDevice.GetConfigDevice().Baud;
            StopBits = selectDevice.GetConfigDevice().StopBits;
            Parity = selectDevice.GetConfigDevice().Parity;
            DataBits = selectDevice.GetConfigDevice().DataBits;
            Dtr = selectDevice.GetConfigDevice().Dtr;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);

            return Task.CompletedTask;
        }

        if (selectDevice is SwitcherMeter)
        {
            standTest.MultiSetConfigSwitcher(TypePort.SerialInput, PortName, Baud, StopBits, Parity,
                DataBits, Dtr);
        }

        if (selectDevice is RelayVip)
        {
            standTest.MultiSetConfigRelayVip(TypePort.SerialInput, PortName, Baud, StopBits, Parity,
                DataBits, Dtr);
            standTest.ConfigMainRelay(TypePort.SerialInput, PortName, Baud, StopBits, Parity,
                DataBits, Dtr);
        }

        AllDevices[index].SetConfigDevice(TypePort.SerialInput, PortName, Baud, StopBits, Parity,
            DataBits, Dtr);


        //для сериализации
        var serializeDevices = AllDevices.ToList();
        serializer.SerializeDevices(serializeDevices);

        DevicesInAllToSort();

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

    /// <summary>
    /// Команда КУДА СОХНРИТЬ файл отчета
    /// </summary>
    public ICommand SaveReportPlaceCmd { get; }

    Task OnSaveReportPlaceCmdExecuted(object p)
    {
        //обработчик команды
        standTest.SaveReportPlace();
        return Task.CompletedTask;
    }

    bool CanSaveReportPlaceCmdExecuted(object p)
    {
        return true;
    }

    /// <summary>
    /// Комнада СОХНРАНИТЬ время 
    /// </summary>
    public ICommand SaveTestAllTimeCmd { get; }

    Task OnSaveTestAllTimeCmdExecuted(object p)
    {
        //обработчик команды
        standTest.SetTimesTest(TestAllTime, TestIntervalTime);
        return Task.CompletedTask;
    }

    bool CanSaveTestAllTimeCmdExecuted(object p)
    {
        return true;
    }

    /// <summary>
    /// Команда ДОБАВИТЬ команду к устройству
    /// </summary>
    public ICommand AddCmdFromDeviceCmd { get; }

    Task OnAddCmdFromDeviceCmdExecuted(object p)
    {
        libCmd.AddCommand(NameCmdLib, SelectDevice.Name, TransmitCmdLib, ReceiveCmdLib, DelayCmdLib,
            StartStingCmdLib, EndStringCmdLib, PingCountCmdLib, TypeMessageCmdLib, IsXor);

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

    /// <summary>
    /// Команда Удалить команду устройства
    /// </summary>
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

    //

    #region Поля

    #region Общие

    /// <summary>
    /// Выключить все вкладки
    /// </summary>
    void TabsDisable()
    {
        PrimaryCheckDevicesTab = false;
        PrimaryCheckVipsTab = false;
        CheckVipsTab = false;
        SettingsTab = false;
    }

    /// <summary>
    /// Выключить все вкладки
    /// </summary>
    void TabsEnable()
    {
        PrimaryCheckDevicesTab = true;
        PrimaryCheckVipsTab = true;
        CheckVipsTab = true;
        SettingsTab = true;
    }


    private TypeOfTestRun testRun;

    /// <summary>
    /// Уведомляет чей сейчас тест идет и управляет поведением формы (текст текущего теста,
    /// процент теущего теста, вкладки тестов
    /// </summary>
    public TypeOfTestRun TestRun
    {
        get => testRun;
        set
        {
            if (!Set(ref testRun, value)) return;

            if (testRun == TypeOfTestRun.Stop)
            {
                TextCurrentTest = "Стенд остановлен";
                PercentCurrentTest = 0;
                TextCurrentTestDevice = "";
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

            //

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

            //

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

            //

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
    /// Уведомляет текстом какое устройство проходит тест
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
    /// Какая сейчас выбрана вкладка
    /// </summary>
    public double SelectTab
    {
        get => selectTab;

        set => Set(ref selectTab, value);
    }

    #endregion

    //

    #region Поля Подключение устройств

    private bool primaryCheckDevicesTab;

    /// <summary>
    /// Включатель вкладки подключения устройств 0
    /// </summary>
    public bool PrimaryCheckDevicesTab
    {
        get => primaryCheckDevicesTab;
        set => Set(ref primaryCheckDevicesTab, value);
    }

    private bool primaryCheckVipsTab;

    /// <summary>
    /// Включатель вкладки предварительной проверки випов 1
    /// </summary>
    public bool PrimaryCheckVipsTab
    {
        get => primaryCheckVipsTab;
        set => Set(ref primaryCheckVipsTab, value);
    }

    private bool checkVipsTab;

    /// <summary>
    /// Включатель влкадки проверки випов 2
    /// </summary>
    public bool CheckVipsTab
    {
        get => checkVipsTab;
        set => Set(ref checkVipsTab, value);
    }

    private bool settingsTab;

    /// <summary>
    /// Включатель влкадки  настроек 3
    /// </summary>
    public bool SettingsTab
    {
        get => settingsTab;
        set => Set(ref settingsTab, value);
    }

    #endregion

    //

    #region Поля подключение ВИПов

    public TypeVip selectTypeVip;

    /// <summary>
    /// Выбор типа Випа
    /// </summary>
    public TypeVip SelectTypeVip
    {
        get { return selectTypeVip; }
        set
        {
            if (!Set(ref selectTypeVip, value)) return;

            try
            {
                standTest.SetTypeVips(SelectTypeVip);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    #endregion

    //

    #region Поля Настройки

    #region Настройки выбранного устройства

    private bool enabledDeviceName;

    /// <summary>
    /// Для отключения имени в случае с релейными модулями тк их имена неизменны
    /// </summary>
    public bool EnabledDeviceName
    {
        get => enabledDeviceName;
        set { Set(ref enabledDeviceName, value); }
    }

    private BaseDevice selectDevice;

    /// <summary>
    /// Выбор устройства в в выпадающем списке
    /// </summary>
    public BaseDevice SelectDevice
    {
        get { return selectDevice; }
        set
        {
            if (!Set(ref selectDevice, value)) return;


            NameDevice = selectDevice.Name;

            try
            {
                PortName = selectDevice.GetConfigDevice().PortName;
                Baud = selectDevice.GetConfigDevice().Baud;
                StopBits = selectDevice.GetConfigDevice().StopBits;
                Parity = selectDevice.GetConfigDevice().Parity;
                DataBits = selectDevice.GetConfigDevice().DataBits;
                Dtr = selectDevice.GetConfigDevice().Dtr;

                //если устройство типа релейного модуля ОТКЛЮЧАЕМ возмонжность изменить его имя
                if (selectDevice is SwitcherMeter)
                {
                    EnabledDeviceName = false;
                }
                //если устройство типа Реле Випа ОТКЛЮЧАЕМ возмонжность изменить его имя
                else if (selectDevice is RelayVip)
                {
                    EnabledDeviceName = false;
                }
                //если устройство НЕ типа релейного модуля ВКЛЮЧАЕМ возмонжность изменить его имя
                else if (selectDevice is not SwitcherMeter)
                {
                    EnabledDeviceName = true;
                }

                //обновление команд выбранного устройства
                selectedDeviceCommand.Source =
                    value?.LibCmd.DeviceCommands.Where(x =>
                        x.Key.NameDevice == selectDevice.Name);
                OnPropertyChanged(nameof(SelectedDeviceCommand));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

    private readonly CollectionViewSource selectedDeviceCommand = new();

    /// <summary>
    /// Для показа/обновление команд выбранного устройства
    /// </summary>
    public ICollectionView? SelectedDeviceCommand => selectedDeviceCommand?.View;

    private bool isXor;

    /// <summary>
    /// Имя устройства в текстбоксе
    /// </summary>
    public bool IsXor
    {
        get => isXor;
        set { Set(ref isXor, value); }
    }


    private string nameDevice;

    /// <summary>
    /// Имя устройства в текстбоксе
    /// </summary>
    public string NameDevice
    {
        get => nameDevice;
        set { Set(ref nameDevice, value); }
    }


    private string portName;

    /// <summary>
    /// Имя порта в текстбоксе
    /// </summary>
    public string PortName
    {
        get => portName;
        set => Set(ref portName, value);
    }

    private int baud;

    /// <summary>
    /// Baud rate порта в текстбоксе 
    /// </summary>
    public int Baud
    {
        get => baud;
        set => Set(ref baud, value);
    }

    private int stopBits;

    /// <summary>
    /// Стоповые биты порта в текстбоксе
    /// </summary>
    public int StopBits
    {
        get => stopBits;
        set => Set(ref stopBits, value);
    }

    private int parity;

    /// <summary>
    /// Parity bits порта в тектсбоксе
    /// </summary>
    public int Parity
    {
        get => parity;
        set => Set(ref parity, value);
    }

    private int dataBits;

    /// <summary>
    /// Бит данных в команде в текстбоксе
    /// </summary>
    public int DataBits
    {
        get => dataBits;
        set => Set(ref dataBits, value);
    }

    private bool dtr;

    /// <summary>
    /// DTR порта в чекбоксе
    /// </summary>
    public bool Dtr
    {
        get => dtr;
        set => Set(ref dtr, value);
    }

    #endregion

    //

    #region Настройки времени

    private TimeSpan testAllTime;

    /// <summary>
    /// Установка общего времени замеров
    /// </summary>
    public TimeSpan TestAllTime
    {
        get => testAllTime;
        set => Set(ref testAllTime, value);
    }

    private TimeSpan testIntervalTime;

    /// <summary>
    /// Установка времени интервалов замеров
    /// </summary>
    public TimeSpan TestIntervalTime
    {
        get => testIntervalTime;
        set => Set(ref testIntervalTime, value);
    }


    private TimeSpan testLeftEndTime;

    /// <summary>
    /// Вроемя конца замеров (последнй замер)
    /// </summary>
    public TimeSpan TestLeftEndTime
    {
        get => testLeftEndTime;
        set => Set(ref testLeftEndTime, value);
    }

    #endregion

    //

    #region Добавление команды

    private string nameCmdLib;

    /// <summary>
    /// Имя команды из библиотеки
    /// </summary>
    public string NameCmdLib
    {
        get => nameCmdLib;
        set => Set(ref nameCmdLib, value);
    }

    private string transmitCmdLib;

    /// <summary>
    /// Отправляемое сообщение для устройства из библиотеки
    /// </summary>
    public string TransmitCmdLib
    {
        get => transmitCmdLib;
        set => Set(ref transmitCmdLib, value);
    }

    private string receiveCmdLib;

    /// <summary>
    /// Принимаемое сообщение из устройства из библиотеки 
    /// </summary>
    public string ReceiveCmdLib
    {
        get => receiveCmdLib;
        set => Set(ref receiveCmdLib, value);
    }

    private string terminatorCmdLib;

    /// <summary>
    /// Разделитель команды из библиотеки
    /// </summary>
    public string TerminatorCmdLib
    {
        get => terminatorCmdLib;
        set => Set(ref terminatorCmdLib, value);
    }

    private TypeCmd typeMessageCmdLib;

    /// <summary>
    /// Тип отправялемемой и принимаемой команды из библиотеки
    /// </summary>
    public TypeCmd TypeMessageCmdLib
    {
        get => typeMessageCmdLib;
        set => Set(ref typeMessageCmdLib, value);
    }

    private int delayCmdLib;

    /// <summary>
    /// ЗАдержка на после отправки команды до ее приема из библиотеки
    /// </summary>
    public int DelayCmdLib
    {
        get => delayCmdLib;
        set => Set(ref delayCmdLib, value);
    }

    private int pingCountCmdLib;

    /// <summary>
    /// Количество пингов устройству (используется в GodSerialPort) из библиотеки
    /// </summary>
    public int PingCountCmdLib
    {
        get => pingCountCmdLib;
        set => Set(ref pingCountCmdLib, value);
    }

    private string startStingCmdLib;

    /// <summary>
    /// С чего начниается команда устройству (используется в GodSerialPort) из библиотеки
    /// </summary>
    public string StartStingCmdLib
    {
        get => startStingCmdLib;
        set => Set(ref startStingCmdLib, value);
    }

    private string endStringCmdLib;

    /// <summary>
    /// Чем заканчаиваетя команда устройству (используется в GodSerialPort) из библиотеки
    /// </summary>
    public string EndStringCmdLib
    {
        get => endStringCmdLib;
        set => Set(ref endStringCmdLib, value);
    }

    private KeyValuePair<DeviceIdentCmd, DeviceCmd> selectedCmdLib;

    /// <summary>
    /// Выбранный итем из библиотеки
    /// </summary>
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