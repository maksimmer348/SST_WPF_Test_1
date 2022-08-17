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

    public void CheckedConnectRelay(string deviceName)
    {
        var selectCmd = GetLibItem("Status", deviceName);

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
                ConnectDevice?.Invoke(relays[0], true);
            }
        }

        else if (receive.Substring(2).Contains("ae"))
        {
            var cmdInLib = GetLibItem("Status", "2");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ConnectDevice?.Invoke(relays[1], true);
            }
            
        }

        else if (receive.Substring(2).Contains("af"))
        {
            var cmdInLib = GetLibItem("Status", "3");
            if (receive.Substring(4).Contains(cmdInLib.Receive))
            {
                ConnectDevice?.Invoke(relays[2], true);
            }
           
        }
    }
}