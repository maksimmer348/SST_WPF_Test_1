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

    public void TransmitCmdInLib(RelayVip device, string cmd)
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
        if (receive.Substring(2).Contains("ad"))
        {
            var cmdInLib = GetLibItem("Status", "1");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ((RelayVip)relays[0]).IdName = "ad";
                ConnectDevice?.Invoke(relays[0], true);
            }
        }

        else if (receive.Substring(2).Contains("ae"))
        {
            var cmdInLib = GetLibItem("Status", "2");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ((RelayVip)relays[1]).IdName = "ae";
                ConnectDevice?.Invoke(relays[1], true);
            }
        }

        else if (receive.Substring(2).Contains("af"))
        {
            var cmdInLib = GetLibItem("Status", "3");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ((RelayVip)relays[2]).IdName = "af";
                ConnectDevice?.Invoke(relays[2], true);
            }
        }

        else if (receive.Substring(2).Contains("b0"))
        {
            var cmdInLib = GetLibItem("Status", "4");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ((RelayVip)relays[2]).IdName = "b0";
                ConnectDevice?.Invoke(relays[3], true);
            }
        }
        
        else if (receive.Substring(2).Contains("b9"))
        {
            var cmdInLib = GetLibItem("Status", "5");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ((RelayVip)relays[2]).IdName = "af";
                ConnectDevice?.Invoke(relays[4], true);
            }
        }
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