using System;
using System.Text;
using System.Threading;
using GodSharp.SerialPort;

namespace SST_WPF_Test_1;

public class SerialGod : ISerialLib
{
    private GodSerialPort port;
    public bool Dtr { get; set; }
    public string GetPortNum { get; set; }

    public int Delay { get; set; }
    public bool IsConnect { get; set; }
    public Action<bool> ConnectionStatusChanged { get; set; }
    public Action<string> MessageReceived { get; set; }


    public void SetPort(string pornName, int baud, int stopBits, int parity, int dataBits, bool dtr = false)
    {
        try
        {
            port = new GodSerialPort(pornName, baud, parity, dataBits, stopBits);
            port.DtrEnable = true;
            GetPortNum = pornName;
        }
        catch (SerialException e)
        {
            throw new SerialException(
                $"SerialGod exception: Порт \"{GetPortNum}\" не конфигурирован, ошибка - {e.Message}");
        }
    }

    public bool Connect()
    {
        try
        {
            var isConnect = port.Open();
            ConnectionStatusChanged?.Invoke(isConnect);

            return isConnect;
        }
        catch (SerialException e)
        {
            throw new SerialException($"SerialGod exception: Порт \"{GetPortNum}\" не отвечает, ошибка - {e.Message}");
        }
    }

    public bool IsOpen()
    {
        if (port != null)
        {
            return port.IsOpen;
        }

        return false;
    }

    public void Disconnect()
    {
        try
        {
            port.Close();
        }
        catch (Exception e)
        {
            throw new SerialException($"SerialGod exception: Порт \"{GetPortNum}\" не отвечает, ошибка - {e.Message}");
        }
    }


    // "0A""0D"
    private string[] trashStr = {"FE", "FC", "F8", "FF", "F0", "E0", " ", "C0"};

    public string ReadPSP(string startOfString, string endOfString = "", int innerCount = 10, int countReads = 3)
    {
        int innerNullCount = innerCount;
        int innerErrorCount = innerCount;

        for (int i = 0; i < countReads; i++)
        {
            var s = port.ReadString();
            //если не прочиатлась строка 
            while (s == null)
            {
                //пытаемся еще раз прочиать
                s = port.ReadString();
                Thread.Sleep(10);
                //если innerCount раз не прочиатлась то вызов исключения
                innerNullCount--;

                if (innerNullCount == 0)
                {
                    throw new SerialException($"SerialGod exception: Ответа нет (null), удачных попыток {innerCount}");
                }
            }

            //удаляем мусори и пробелы из строки
            foreach (var str in trashStr)
            {
                s = s.Replace(str, "");
            }

            //если строка содержит входной символ
            while (s.Contains(startOfString))
            {
                //то мы прибавляем строку из буффера компорта
                s += port.ReadString();
                s = s.Replace(" ", "");
                //TODO переделать (0D == \r, 0A == \n)
                //проверяемем есть ли в строке сиволы окончания
                if (string.Equals(s.Substring(s.Length - endOfString.Length), endOfString,
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    //проверка на дублирование ответа если дублирован убираем 2 половину
                    if (s.Substring(0, s.Length / 2) == s.Substring(s.Length / 2, s.Length / 2))
                    {
                        s = s.Substring(0, s.Length / 2);
                    }

                    //возвращаем строку
                    MessageReceived.Invoke(s);
                    return s;
                }

                innerErrorCount--;
                //тк мы в while если количевто попыток прочиать строку превысит 10 попыток то выходим из цикла
                //и выбрасываем исключение
                if (innerErrorCount <= 0)
                {
                    //TODO раскоменетить
                    //throw new Exception($"SerialGod exception: Слишком много неудачых попыток (notNull) - {innerCount}");
                    Console.WriteLine($"SerialGod exception: Слишком много неудачых попыток (notNull) - {innerCount}");
                }

                Thread.Sleep(10);
            }

            if (!s.Contains(startOfString))
            {
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                Thread.Sleep(20);
                TransmitCmdTextString(startOfString);
            }
        }

        throw new Exception($"SerialGod exception: Ответа нет, неудачых попыток {countReads}");
    }

    public void TransmitCmdTextString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            throw new SerialException($"SerialGod exception: При ручном вводе, checkCmd- не должны быть пустыми");
        }

        if (string.IsNullOrEmpty(terminator))
        {
            terminator = "0A0D";
        }

        Delay = delay;
        try
        {
            port.WriteHexString(cmd + terminator);
            Thread.Sleep(30);
            ReadPSP(start, end);
        }
        catch (SerialException e)
        {
            throw new SerialException("SerialGod exception: " + e.Message);
        }
    }

    public void TransmitCmdHexString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null)
    {
        TransmitCmdTextString(GetStringHexInText(cmd), delay,
            GetStringHexInText(start), GetStringHexInText(end),
            GetStringHexInText(terminator));
    }

    //TODO убрать расплодившиеся методы
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