using System;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RJCP.IO.Ports;
using SerialPortLib;


namespace SST_WPF_Test_1;

public class SerialInput : ISerialLib
{
    protected SerialPortInput port;
    public bool Dtr { get; set; }
    public string GetPortNum { get; set; }
    public int Delay { get; set; }
    
    
    public Action<bool> ConnectionStatusChanged { get; set; }
    public Action<string> MessageReceived { get; set; }

    public void SetPort(string pornName, int baud, int stopBits, int parity, int dataBits, bool dtr = false)
    {
        var adaptSettings = SetPortAdapter(stopBits, parity, dataBits);
        port = new SerialPortInput(new NullLogger<SerialPortInput>());
        port.ConnectionStatusChanged += OnPortConnectionStatusChanged;
        port.MessageReceived += OnPortMessageReceived;
        try
        {
            port.SetPort(pornName, baud, adaptSettings.Item1, adaptSettings.Item2, adaptSettings.Item3);
        }
        catch (SerialException e)
        {
            throw new SerialException(
                $"SerialInput exception: Порт \"{GetPortNum}\" не конфигурирован, ошибка - {e.Message}");
        }

        GetPortNum = pornName;
    }

    public bool Open()
    {
        if (!port.IsConnected)
        {
         return port.Connect();
        }

        return false;
    }

    public void Close()
    {
        if (port != null)
        {
            if (port.IsConnected)
            {
                port.Disconnect();
            }
        }
    }
    
    
    /// <summary>
    /// Прием ответа соединения от serial port
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">Наличие коннекта true/false</param>
    public void OnPortConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
    {
        ConnectionStatusChanged.Invoke(args.Connected);
    }

    /// <summary>
    /// Прием сообщения из устройства
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">Сообщение от устройства</param>
    public void OnPortMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        var data = System.Text.Encoding.UTF8.GetString(args.Data);
        var answer = (data);
        MessageReceived.Invoke(answer);
    }
    
    /// <summary>
    /// Адаптер значений для библиотеки 
    /// </summary>
    /// <param name="sBits">Stop bits (1-2)</param>
    /// <param name="par">Patyty bits (0-2)</param>
    /// <param name="dBits">Data bits (5-8)</param>
    /// <returns></returns>
    public (StopBits, Parity, DataBits) SetPortAdapter(int sBits, int par, int dBits)
    {
        StopBits stopBits = StopBits.One;
        Parity parity = Parity.None;
        DataBits dataBits = DataBits.Eight;

        stopBits = sBits switch
        {
            1 => StopBits.One,
            2 => StopBits.Two,
            _ => stopBits
        };

        parity = par switch
        {
            0 => Parity.None,
            1 => Parity.Odd,
            2 => Parity.Even,
            _ => parity
        };
        dataBits = dBits switch
        {
            5 => DataBits.Five,
            6 => DataBits.Six,
            7 => DataBits.Seven,
            8 => DataBits.Eight,
            _ => dataBits
        };
        return (stopBits, parity, dataBits);
    }

    /// <summary>
    /// Отправка в устройство и прием команд из устройства
    /// </summary>
    /// <param name="cmd">Команда</param>
    /// <param name="delay">Задержка между запросом и ответом</param>
    /// <param name="receiveType"></param>
    /// <param name="terminator">Окончание строки команды по умолчанию \r\n</param>
    /// <returns>Ответ от устройства</returns>
    public void TransmitCmdTextString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            throw new SerialException($"SerialInput exception: Команда - не должны быть пустыми");
        }

        if (delay == 0)
        {
            throw new SerialException($"SerialInput exception: Задержка - не должны быть = 0");
        }

        if (string.IsNullOrEmpty(terminator))
        {
            terminator = "\r\n";
        }

        Delay = delay;
        var message = System.Text.Encoding.UTF8.GetBytes(cmd + terminator);
        port.SendMessage(message);
    }

    /// <summary>
    /// Отправка в прибор
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="delay"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="terminator"></param>
    public void TransmitCmdHexString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null)
    {
        TransmitCmdTextString(GetStringHexInText(cmd), delay,
            GetStringHexInText(start), GetStringHexInText(end),
            GetStringHexInText(terminator));
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

    string GetStringHexInText(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            string hex = "";
            foreach (var ss in s)
            {
                hex += Convert.ToByte(ss).ToString("x2");
            }

            return hex;
        }

        return "";
    }
}