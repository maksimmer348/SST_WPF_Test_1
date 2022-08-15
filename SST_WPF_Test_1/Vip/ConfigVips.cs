using System;
using System.Collections.ObjectModel;

namespace SST_WPF_Test_1;

public class ConfigVips
{
    private MainValidator mainValidator = new();
    public ObservableCollection<Vip> Vips { get; set; } = new ObservableCollection<Vip>();
    public ObservableCollection<TypeVip> TypeVips { get; set; } = new ObservableCollection<TypeVip>();

    #region Типы Випов

    //TODO испрвить на реальные значения
    /// <summary>
    /// Предваритльное добавление типов Випов
    /// </summary>
    public void PrepareAddTypeVips()
    {
        var typeVip71 = new TypeVip
        {
            Type = "Vip71",
            //максимаьные значения во время испытаниий они означают ошибку
            MaxTemperature = 90,
            MaxVoltageIn = 120,
            MaxVoltageOut1 = 20,
            MaxVoltageOut2 = 25,
            MaxCurrentIn = 5,
            //максимальные значения во время предпотготовки испытания (PrepareMaxVoltageOut1 и PrepareMaxVoltageOut2
            //берутся из MaxVoltageOut1 и MaxVoltageOut2 соотвественно)
            PrepareMaxCurrentIn = 0.5,
            //настройки для приборов они зависят от типа Випа
        };

        typeVip71.SetDeviceParameters( Random.Shared.Next(100,200), 3.3, 1.65, 20, 20, 5);
        AddTypeVips(typeVip71);

        var typeVip70 = new TypeVip
        {
            Type = "Vip70",
            MaxTemperature = 70,
            //максимаьные значения во время испытаниий они означают ошибку
            MaxVoltageIn = 220,
            MaxVoltageOut1 = 40,
            MaxVoltageOut2 = 45,
            MaxCurrentIn = 2.5,
            //максимальные значения во время предпотготовки испытания (PrepareMaxVoltageOut1 и PrepareMaxVoltageOut2
            //берутся из MaxVoltageOut1 и MaxVoltageOut2 соотвественно)
            PrepareMaxCurrentIn = 0.5,
            //настройки для приборов они зависят от типа Випа
            
        };
       
       typeVip70.SetDeviceParameters(Random.Shared.Next(200,300), 4, 2, 10, 30, 10);
       AddTypeVips(typeVip70);
    }

    /// <summary>
    /// Тип випа от него зависит его предварительные и рабочие макс значения  
    /// </summary>
    /// <param name="type">Не удалось добавить новый тип випа</param>
    public void AddTypeVips(TypeVip type)
    {
        try
        {
            TypeVips.Add(type);
            Console.WriteLine($"Создан тип Випа {type.Type}, максимальная тепмпература {type.MaxTemperature}," +
                              $" максимальнный предварительный ток 1 {type.PrepareMaxVoltageOut1}, " +
                              $"максимальнный предварительный ток 2 {type.PrepareMaxVoltageOut2}");
            //уведомить
        }
        catch (Exception e)
        {
            throw new VipException($"Не создан тип Випа {type.Type}, ошибка{e}");
        }
    }

    public void RemoveTypeVips(int indextypeVip)
    {
        try
        {
            Console.WriteLine($"Удален тип Випа {TypeVips[indextypeVip]}");
            TypeVips.RemoveAt(indextypeVip);
            //уведомить
        }
        catch (Exception e)
        {
            throw new VipException($"Не удален тип Випа {TypeVips[indextypeVip]}, ошибка{e}");
        }
    }

    public void ChangedTypeVips(int indextypeVip, TypeVip newTypeVips)
    {
        try
        {
            //Console.WriteLine($"До изменения типа Випа {TypeVips[indextypeVip].PrepareMaxVoltageOut1}, {TypeVips[indextypeVip].PrepareMaxVoltageOut2}");
            Console.WriteLine(
                $"До изменения типа Випа {TypeVips[indextypeVip].MaxVoltageOut1}, {TypeVips[indextypeVip].MaxVoltageOut2}");
            TypeVips[indextypeVip] = newTypeVips;
            //Console.WriteLine($"После изменения тип Випа {TypeVips[indextypeVip].PrepareMaxVoltageOut1}, {TypeVips[indextypeVip].PrepareMaxVoltageOut2}");
            Console.WriteLine(
                $"После изменения тип Випа {TypeVips[indextypeVip].MaxVoltageOut1}, {TypeVips[indextypeVip].MaxVoltageOut2}");
        }
        catch (Exception e)
        {
            throw new VipException($"Не изменен тип Випа {TypeVips[indextypeVip]}, ошибка{e}");
        }
    }

    #endregion

    #region Добавление и удаление Випов

    /// <summary>
    /// Доабавить новый Вип
    /// </summary>
    /// <param name="name">Имя Випа (Берется из текстбокса)</param>
    /// <param name="indexTypeVip">Тип Випа (берется из списка который будет привязан к индексу сомбобокса)</param>
    [ObsoleteAttribute("Метод больше не используется")]
    public void AddVip(string name, int indexTypeVip, int id)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            // проверка на недопуст символы 
            if (!mainValidator.ValidateInvalidSymbols(name))
            {
                //TODO уточнить где кидать исключение здесь или в классе MainValidator
                //TODO сделать чтобы исключение выбрасывалось при потере контекста в текстбоксе
                throw new VipException($"Название добавляемого Випа - {name}, содержит недопустимые символы");
            }

            // проверка на повторяющиеся имена Випов 
            if (!mainValidator.ValidateCollisionName(name, Vips))
            {
                //TODO уточнить где кидать исключение здесь или в классе MainValidator
                //TODO сделать чтобы исключение выбрасывалось при потере контекста в текстбоксе
                throw new VipException($"Название добавляемого Випа - {name}, уже есть в списке");
            }

            var vip = new Vip(id)
            {
                Name = name,
                Type = TypeVips[indexTypeVip],
                StatusTest = StatusDeviceTest.None
            };
            Vips.Add(vip);
            Console.WriteLine("Вип имя: " + vip.Name + " был добалвен");
            //уведомить
        }
    }

    //TODO должно срабоать при удалении текста из текстбокса 
    /// <summary>
    /// Удаление Випа
    /// </summary>
    /// <param name="indexVip">Индекс Випа (берется из списка который будет привязан к индексу сомбобокса)</param>
    [ObsoleteAttribute("Метод больше не используется")]
    public void RemoveVip(Vip vip)
    {
        try
        {
            Vips.Remove(vip);
            Console.WriteLine("Вип : " + vip.Name + " был удален");
            //уведомить
        }
        catch (VipException e)
        {
            throw new VipException("Вип c индексом: " + vip.Name + "не был был удален");
        }
    }

    #endregion
}