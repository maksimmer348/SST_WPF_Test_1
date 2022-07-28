using System;

namespace SST_WPF_Test_1;

//Основной обработчик команд
public class ActionCommand : BaseCommand
{
    //эти два делегата должны быть связаны с одноименными методами
    private readonly Action<object> execute;
    private readonly Func<object, bool> canExecute;

    //здесь нужно получитиь два дегелата кторые будут выполнятся метотом CanExecute и Execute
    //те указать 2 действия которые команда может выыполнять
    public ActionCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        //если не передаели ссылку на делегат то получаем исключение
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    //связываем метод с одноименным делегатом
    //если делегата нет то команду можно выполнить в люббом случае (доступность команды)
    public override bool CanExecute(object? parameter) =>
        canExecute?.Invoke(parameter) ?? true;

    //связываем метод с одноименным делегатом (логика команды)
    public override void Execute(object? parameter) => execute(parameter);
}