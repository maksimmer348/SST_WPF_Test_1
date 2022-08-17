using System;
using Newtonsoft.Json;

namespace SST_WPF_Test_1;

/// <summary>
/// Стандартная команда, ответ и задержка
/// </summary>
public class DeviceCmd
{
    /// <summary>
    /// Команда в устройство
    /// </summary>
    [JsonProperty("transmit")]
    public string Transmit { get; set; }

    /// <summary>
    /// Окончание строки
    /// </summary>
    [JsonProperty("terminator")]
    public string Terminator { get; set; }

    /// <summary>
    /// Ответ от устройства
    /// </summary>
    [JsonProperty("receive")]
    public string Receive { get; set; }

    /// <summary>
    /// Тип команды  и ответа от устройства (hex/text) 
    /// </summary>
    [JsonProperty("messageType")]
    public TypeCmd MessageType { get; set; }

    /// <summary>
    /// Задержка между передачей команды и приемом ответа 
    /// </summary>
    [JsonProperty("delay")]
    public int Delay { get; set; }

    /// <summary>
    ///  Количество Запросов на прибор (используется в библиотеке SerialGod)
    /// </summary>
    [JsonProperty("pingCount")]
    public int PingCount { get; set; }

    /// <summary>
    ///  Начало строки (используется в библиотеке SerialGod)
    /// </summary>
    [JsonProperty("startOfString")]
    public string StartOfString { get; set; }

    /// <summary>
    ///  Конец строки (используется в библиотеке SerialGod)
    /// </summary>
    [JsonProperty("endOfString")]
    public string EndOfString { get; set; }

    /// <summary>
    ///  Производитль ли над командой xor операцию
    /// </summary>
    [JsonProperty("isXor")]
    public bool IsXor { get; set; }

    protected bool Equals(DeviceCmd other)
    {
        return Transmit == other.Transmit && Terminator == other.Terminator && Receive == other.Receive &&
               MessageType == other.MessageType && Delay == other.Delay && PingCount == other.PingCount &&
               StartOfString == other.StartOfString && EndOfString == other.EndOfString && IsXor == other.IsXor;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DeviceCmd)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Transmit);
        hashCode.Add(Terminator);
        hashCode.Add(Receive);
        hashCode.Add((int)MessageType);
        hashCode.Add(Delay);
        hashCode.Add(PingCount);
        hashCode.Add(StartOfString);
        hashCode.Add(EndOfString);
        hashCode.Add(IsXor);
        return hashCode.ToHashCode();
    }
    // protected bool Equals(DeviceCmd other)
    // {
    //     return Transmit == other.Transmit && Terminator == other.Terminator && Receive == other.Receive &&
    //            MessageType == other.MessageType && Delay == other.Delay &&
    //            PingCount == other.PingCount && StartOfString == other.StartOfString &&
    //            EndOfString == other.EndOfString && other.IsXor;
    // }
    //
    // public override bool Equals(object? obj)
    // {
    //     if (ReferenceEquals(null, obj)) return false;
    //     if (ReferenceEquals(this, obj)) return true;
    //     if (obj.GetType() != this.GetType()) return false;
    //     return Equals((DeviceCmd) obj);
    // }
    //
    // public override int GetHashCode()
    // {
    //     return HashCode.Combine(Transmit, Terminator, Receive, (int) MessageType, Delay, PingCount,
    //         StartOfString, EndOfString);
    // }
}