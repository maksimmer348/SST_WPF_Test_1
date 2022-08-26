using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SST_WPF_Test_1;

public class MainRelay : BaseDevice
{
    private List<BaseDevice> relays;

    public ISerialLib GetPort()
    {
        if (port != null)
        {
            return port;
        }
        else
        {
            throw new DeviceException($"Порт на устройстве {Name} еще нe был инициализирован");
        }
    }

    public MainRelay(string name, ObservableCollection<BaseDevice> relaysVips) : base(name)
    {
        relays = relaysVips.ToList();
        ReceiveRelay += ReceiveRelayMessage;
    }

    public void TransmitCmdInLibRelay(RelayVip device, string cmd)
    {
        var selectCmd = GetLibItem(cmd, device.Name);

        if (selectCmd == null)
        {
            throw new DeviceException(
                $"BaseDevice exception: Такое устройство - {IsDeviceType}/{Name} или команда - \"Status\" в библиотеке не найдены");
        }

        CmdDelay = selectCmd.Delay;

        if (selectCmd.MessageType == TypeCmd.Hex)
        {
            TypeReceive = TypeCmd.Hex;
            if (selectCmd.IsXor)
            {
                port.TransmitCmdHexString(selectCmd.Transmit, selectCmd.Delay,
                    selectCmd.StartOfString, selectCmd.EndOfString,
                    selectCmd.Terminator, true);
            }
            else
            {
                port.TransmitCmdHexString(selectCmd.Transmit, selectCmd.Delay,
                    selectCmd.StartOfString, selectCmd.EndOfString,
                    selectCmd.Terminator);
            }
        }
        else
        {
            TypeReceive = TypeCmd.Text;
            port.TransmitCmdTextString(selectCmd.Transmit, selectCmd.Delay, selectCmd.StartOfString,
                selectCmd.EndOfString,
                selectCmd.Terminator);
        }
    }

    private void ReceiveRelayMessage(string receive)
    {
        (KeyValuePair<DeviceIdentCmd, DeviceCmd> cmd, BaseDevice baseDevice) cmdInLib = 
            (new KeyValuePair<DeviceIdentCmd, DeviceCmd>(), null);
        
        try
        {
            var addrVip = receive.Substring(2, 2); //TODO если строка неправильной длины поробовать еще раз
            var cmdVip = receive.Substring(4, 2); //TODO если строка неправильной длины поробовать еще раз

            cmdInLib = RelayLibEncode(cmdVip, addrVip);
            
            if (cmdInLib.cmd.Value.Receive == cmdVip)
            {
                ConnectDevice?.Invoke(cmdInLib.baseDevice, true);
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            ConnectDevice?.Invoke(cmdInLib.baseDevice, false);
            return;
        }
        catch (Exception e)
        {
            ConnectDevice?.Invoke(cmdInLib.baseDevice, false);
            return;
        }
    }

    private (KeyValuePair<DeviceIdentCmd, DeviceCmd> cmd, BaseDevice device) RelayLibEncode(string cmdVip,
        string vipName)
    {
        (KeyValuePair<DeviceIdentCmd, DeviceCmd> cmd, BaseDevice baseDevice) cmdInLib = 
        (new KeyValuePair<DeviceIdentCmd, DeviceCmd>(), null);
        switch (vipName)
        {
            case "ad":
            {
                cmdInLib = GetLibItemInReceive(cmdVip, "1", relays);
                break;
            }
            case "ae":
            {
                cmdInLib = GetLibItemInReceive(cmdVip, "2", relays);
                break;
            }
            case "af":
            {
                cmdInLib = GetLibItemInReceive(cmdVip, "3", relays);
                break;
            }
            case "b0":
            {
                cmdInLib = GetLibItemInReceive(cmdVip, "4", relays);
                break;
            }
            case "b9":
            {
                cmdInLib = GetLibItemInReceive(cmdVip, "5", relays);
                break;
            }
        }

        return cmdInLib;
    }

    // public void EnabledRelay(BaseDevice device)
    // {
    //     var selectCmd = GetLibItem("Status", device.Name);
    //     TransmitCmdInLib();
    // }

    // public string RelayTransmitCmdInLib(RelayVip vipRelay, string cmd)
    // {
    //     var selectCmd = GetLibItem(cmd, vipRelay.Name);
    //     
    // }
}