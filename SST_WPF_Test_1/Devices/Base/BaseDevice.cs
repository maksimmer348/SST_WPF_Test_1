using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class BaseDevice : Notify
{
    private string isDeviceType;

    public string IsDeviceType
    {
        get => isDeviceType;
        set => Set(ref isDeviceType, value);
    }
    private string name;
    /// <summary>
    /// Имя прибора
    /// </summary>
    public string Name
    {
        get => name;
        set => Set(ref name, value);
    }

    public bool IsConnect { get; set; }

    
    private StatusDeviceTest statusTest;

    public StatusDeviceTest StatusTest
    {
        get => statusTest;
        set => Set(ref statusTest, value, nameof(StatusColor));
    }

    public Brush StatusColor =>
        StatusTest switch
        {
            StatusDeviceTest.Error => Brushes.Red,
            StatusDeviceTest.Ok => Brushes.Green,
            _ => Brushes.DarkGray
        };
    
    public ConfigDeviceParams Config { get; set; } = new ConfigDeviceParams();

    /// <summary>
    /// Компорт прибора
    /// </summary>
    [field: NonSerialized]
    protected ISerialLib port { get; set; }

    /// <summary>
    /// Класс библиотеки
    /// </summary>
    [NonSerialized]
    public BaseLibCmd LibCmd = BaseLibCmd.getInstance();

    /// <summary>
    /// Класс библиотеки
    /// </summary>
    protected TypeCmd typeReceive { get; set; }

    /// <summary>
    /// Задержка команды
    /// </summary>
    public int CmdDelay { get; set; }

    /// <summary>
    /// Событие проверки коннекта к порту
    /// </summary>
    [field: NonSerialized]
    public Action<BaseDevice, bool> ConnectPort;

    /// <summary>
    /// Событие проверки коннекта к устройству
    /// </summary>
    [field: NonSerialized]
    public Action<BaseDevice, bool> ConnectDevice;


    /// <summary>
    /// Событие приема данных с устройства
    /// </summary>
    [field: NonSerialized]
    public Action<BaseDevice, string> Receive;

    Stopwatch stopwatch = new();

    //
    public int RowIndex { get; set; }

    public int ColumnIndex { get; set; }
    //

    public BaseDevice(string name)
    {
        Name = name;
    }

    public void SetConfigDevice(TypePort typePort, string portName, int baud, int stopBits, int parity, int dataBits,
        bool dtr = true)
    {
        Config.TypePort = typePort;
        Config.PortName = $"{portName}";
        Config.Baud = baud;
        Config.StopBits = stopBits;
        Config.Parity = parity;
        Config.DataBits = dataBits;
        Config.Dtr = dtr;
    }

    public bool Open()
    {
        return port.Open();
    }

    public void Close()
    {
        if (port != null)
        {
            port.Close();
        }
    }
    
    /// <summary>
    /// Конфигурация компортра утройства
    /// </summary>
    /// <param name="typePort">Тип исопльзуемой библиотеки com port</param>
    /// <param name="pornName">Номер компорта</param>
    /// <param name="baud">Бауд рейт компорта</param>
    /// <param name="stopBits">Стоповые биты компорта</param>
    /// <param name="parity">Parity bits</param>
    /// <param name="dataBits">Data bits count</param>
    /// <param name="dtr"></param>
    /// <returns></returns>
    public void Start()
    {
        if (Config.TypePort == TypePort.GodSerial)
        {
            port = new SerialGod();
        }

        if (Config.TypePort == TypePort.SerialInput)
        {
            port = new SerialInput();
        }

        port.ConnectionStatusChanged += ConnectionStatusChanged;
        port.MessageReceived += MessageReceived;
        
        port.SetPort(Config.PortName, Config.Baud, Config.StopBits, Config.Parity, Config.DataBits);
        port.Open();
        
        port.Dtr = Config.Dtr;
    }
    
    public ConfigDeviceParams GetConfigDevice()
    {
        if (Config != null)
        {
            return Config;
        }

        throw new DeviceException("BaseDevice exception: Файл конфига отсутствует");
    }

    /// <summary>
    /// Проверка устройства на коннект
    /// </summary>
    /// <param name="checkCmd">Команда проверки не из библиотеки (если пусто будет исользована команда "Status" и прибор из библиотеки )</param>
    /// <param name="delay">Задержка на проверку (если 0 будет исользована из библиотеки )</param>
    /// <returns>Успешна ли попытка коннекта</returns>
    /// <exception cref="DeviceException">Такого устройства, нет в библиотеке команд</exception>
    public void CheckedConnectDevice(string checkCmd = "", int delay = 0, string terminator = "")
    {
        //для отладки
        // Время начала 
        if (!stopwatch.IsRunning)
        {
            stopwatch.Start();
        }

        //

        //если строка команды пустая
        if (string.IsNullOrWhiteSpace(checkCmd))
        {
            //используем команду статус которя возмет текущий прибор и введет в него команду статус
            TransmitCmdInLib("Status");
        }
        else
        {
            //используем ручной ввод
            port.TransmitCmdTextString(cmd: checkCmd, delay: delay, terminator: terminator);
        }
    }

    /// <summary>
    /// Отправка в устройство и прием СТАНДАРТНЫХ (есть в библиотеке команд) команд из устройства
    /// </summary>
    /// <param name="nameCommand">Имя команды (например Status)</param>
    public void TransmitCmdInLib(string nameCommand)
    {
        // MeterCmd selectCmd = libCmd.DeviceCommands
        //     .FirstOrDefault(x => x.Key.NameCmd == nameCommand && x.Key.NameDevice == Name).Value;
        var selectCmd = GetLibItem(nameCommand, Name);

        if (selectCmd == null)
        {
            throw new DeviceException(
                $"BaseDevice exception: Такое устройство - {Name} или команда - {nameCommand} в библиотеке не найдены");
        }

        CmdDelay = selectCmd.Delay;

        if (selectCmd.MessageType == TypeCmd.Hex)
        {
            typeReceive = TypeCmd.Hex;
            port.TransmitCmdHexString(selectCmd.Transmit, selectCmd.Delay,
                selectCmd.StartOfString, selectCmd.EndOfString,
                selectCmd.Terminator);
        }
        else
        {
            typeReceive = TypeCmd.Text;
            port.TransmitCmdTextString(selectCmd.Transmit, selectCmd.Delay, selectCmd.StartOfString,
                selectCmd.EndOfString,
                selectCmd.Terminator);
        }
    }

    /// <summary>
    /// Выббор команды из библиотеке осноываясь на ее имени и имени прибора
    /// </summary>
    /// <param name="cmd">Имя команды</param>
    /// <param name="deviceName">Имя прибора</param>
    /// <returns></returns>
    protected DeviceCmd GetLibItem(string cmd, string deviceName)
    {
        return LibCmd.DeviceCommands
            .FirstOrDefault(x => x.Key.NameCmd == cmd && x.Key.NameDevice == deviceName).Value;
    }

    /// <summary>
    /// Прошел ли коннект выбраного com port
    /// </summary>
    private void ConnectionStatusChanged(bool isConnect)
    {
        IsConnect = true;
        ConnectPort.Invoke(this, true);
    }

  
    /// <summary>
    /// Обработка прнятого сообщения из устройства
    /// </summary>
    private void MessageReceived(string receive)
    {
        //для проверки на статус 
        var selectCmd = GetLibItem("Status", Name);
        
        if (typeReceive == TypeCmd.Text)
        {
            if (receive.Contains(selectCmd.Receive))
            {
                ConnectDevice.Invoke(this, true);
                return;
            }

            Receive.Invoke(this, receive);
        }

        if (typeReceive == TypeCmd.Hex)
        {
            if (GetStringTextInHex(receive).Contains(selectCmd.Receive))
            {
                ConnectDevice.Invoke(this, true);
                return;
            }

            Receive.Invoke(this, GetStringTextInHex(receive));
        }
    }

    string GetStringTextInHex(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            byte[] bytes = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                var ff = bytes[i / 2];
                bytes[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes);
        }

        return "";
    }
}