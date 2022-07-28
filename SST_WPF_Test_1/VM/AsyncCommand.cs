using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SST_WPF_Test_1;
public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();
    bool CanExecute();
}

