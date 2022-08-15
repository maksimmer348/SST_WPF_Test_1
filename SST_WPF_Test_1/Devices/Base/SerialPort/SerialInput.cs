﻿using System;
using System.Diagnostics;
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
    public Action<byte[]> MessageReceived { get; set; }

    /// <summary>
    /// Адаптер значений для библиотеки 
    /// </summary>
    /// <param name="sBits">Stop bits (1-2)</param>
    /// <param name="par">Parity bits (0-2)</param>
    /// <param name="dBits">Data bits (5-8)</param>open
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
        try
        {
            if (!port.IsConnected)
            {
                Debug.WriteLine($"SerialInput message: {GetPortNum} включаен");
                return port.Connect();
            }

            if (port.IsConnected)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            throw new SerialException(
                $"SerialInput exception: Порт \"{GetPortNum}\" не открыт, ошибка - {e.Message}");
        }

        return false;
    }

    public void Close()
    {
        try
        {
            Debug.WriteLine($"SerialInput message: {GetPortNum} отключен");
            port.Disconnect();
        }
        catch (Exception e)
        {
            throw new SerialException(
                $"SerialInput exception: Порт \"{GetPortNum}\" не закрыт, ошибка - {e.Message}");
        }
    }
    
    public void OnPortConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
    {
        ConnectionStatusChanged.Invoke(args.Connected);
    }
    
    public void OnPortMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        MessageReceived.Invoke(args.Data);
    }
    
    public void TransmitCmdTextString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            throw new SerialException($"SerialInput exception: Команда - не должны быть пустыми");
        }

        if (delay == 0)
        {
            delay = 100;
        }

        if (string.IsNullOrEmpty(terminator))
        {
            terminator = "\r\n";
        }

        Delay = delay;
        var message = System.Text.Encoding.UTF8.GetBytes(cmd + terminator);
        try
        {
            port.SendMessage(message);
        }
        catch (Exception e)
        {
            throw new SerialException(
                $"SerialInput exception: Команда \"{message}\", в порт \"{GetPortNum}\" не отправлена, ошибка - {e.Message}");
        }
    }

    public void TransmitCmdHexString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            throw new SerialException($"SerialInput exception: Команда - не должна быть пустой");
        }

        if (delay == 0)
        {
            delay = 100;
        }

        // if (string.IsNullOrEmpty(terminator))
        // {
        //     terminator = "0A0D";
        // }

        Delay = delay;

        var message = ISerialLib.StringToByteArray(cmd + terminator);
        try
        {
            port.SendMessage(message);
        }
        catch (Exception e)
        {
            throw new SerialException(
                $"SerialInput exception: Команда \"{message}\", в порт \"{GetPortNum}\" не отправлена, ошибка - {e.Message}");
        }
    }
}