using System.Collections.ObjectModel;
using System.Linq;

namespace SST_WPF_Test_1;

public static class ExtensionDevice
{
    public static T GetTypeDevice<T>(this ObservableCollection<BaseDevice> baseDevices) where T : BaseDevice
    {
        return (T)baseDevices.FirstOrDefault(x => x is T);
    }
}