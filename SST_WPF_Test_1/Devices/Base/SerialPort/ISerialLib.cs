using System;

namespace SST_WPF_Test_1;

public interface ISerialLib
{
    /// <summary>
    /// DTR прибора
    /// </summary>
    public bool Dtr { get; set; }

    /// <summary>
    /// Получение номера порта
    /// </summary>
    public string GetPortNum { get; set; }

    /// <summary>
    /// Задержка на выполнение команды/ожидание ответа от устройства
    /// </summary>
    public int Delay { get; set; }

    
    public bool Open();
    public void Close();
    
    /// <summary>
    /// Событие конекта к порту 
    /// </summary>
    Action<bool> ConnectionStatusChanged { get; set; }

    /// <summary>
    /// Событие ответа устройства
    /// </summary>
    Action<string> MessageReceived { get; set; }

    /// <summary>
    /// Настройка порта 
    /// </summary>
    /// <param name="pornName">Имя (например COM32)</param>
    /// <param name="baud">Baud rate (например 2400)</param>
    /// <param name="stopBits">Stop bits (например 1)</param>
    /// <param name="parity">Parity bits (например 0)</param>
    /// <param name="dataBits">Data bits (напрмиер 8)</param>
    /// <param name="dtr">Dtr - по умолчанию false (напрмие true)</param>
    public void SetPort(string pornName, int baud, int stopBits, int parity, int dataBits, bool dtr = false);
    
    //TODO два вида отправки для разных типаов писем
    /// <summary>
    /// Отправка в устройство и прием команд из устройства
    /// </summary>
    /// <param name="cmd">Команда</param>
    /// <param name="delay">Задержка между запросом и ответом</param>
    /// <param name="start">Начало строки для библиотеки SerialGod </param>
    /// <param name="end">Конец строки для библиотеки SerialGod</param>
    /// <param name="terminator">Окончание строки команды - по умолчанию \n\r или 0D0A </param>
    public void TransmitCmdTextString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null);


    public void TransmitCmdHexString(string cmd, int delay = 0, string start = null, string end = null,
        string terminator = null);
}