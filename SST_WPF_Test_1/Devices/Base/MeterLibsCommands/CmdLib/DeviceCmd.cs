using System;
namespace SST_WPF_Test_1;

/// <summary>
/// Стандартная команда, ответ и задержка
/// </summary>
public class DeviceCmd
{
    /// <summary>
    /// Команда в устройство
    /// </summary>
    public string Transmit { get; set; }

    /// <summary>
    /// Окончание строки
    /// </summary>
    public string Terminator { get; set; }

    /// <summary>
    /// Ответ от устройства
    /// </summary>

    public string Receive { get; set; }

    /// <summary>
    /// Тип команды  и ответа от устройства (hex/text) 
    /// </summary>
    public TypeCmd MessageType { get; set; }

    /// <summary>
    /// Задержка между передачей команды и приемом ответа 
    /// </summary>
    public int Delay { get; set; }

    /// <summary>
    ///  Количество Запросов на прибор (используется в библиотеке SerialGod)
    /// </summary>
    public int PingCount { get; set; }

    /// <summary>
    ///  Начало строки (используется в библиотеке SerialGod)
    /// </summary>
    public string StartOfString { get; set; }

    /// <summary>
    ///  Конец строки (используется в библиотеке SerialGod)
    /// </summary>
    public string EndOfString { get; set; }

    // public override int GetHashCode()
    // {
    //     return HashCode.Combine(Transmit, Terminator, Receive, Delay);
    // }
    protected bool Equals(DeviceCmd other)
    {
        return Transmit == other.Transmit && Terminator == other.Terminator && Receive == other.Receive &&
               MessageType == other.MessageType && Delay == other.Delay &&
               PingCount == other.PingCount && StartOfString == other.StartOfString &&
               EndOfString == other.EndOfString;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DeviceCmd) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Transmit, Terminator, Receive, (int) MessageType, Delay, PingCount,
            StartOfString, EndOfString);
    }
}