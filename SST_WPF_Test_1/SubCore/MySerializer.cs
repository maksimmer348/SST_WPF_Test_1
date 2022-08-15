using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class MySerializer
{
    /// <summary>
    /// Сериализуемая библиотека
    /// </summary>
    public Dictionary<DeviceIdentCmd, DeviceCmd> LibCmd;
    
    public List<KeyValuePair<DeviceIdentCmd, DeviceCmd>> SerializedLocations
    {
        get { return LibCmd.ToList(); }
        set { LibCmd = value.ToDictionary(x => x.Key, x => x.Value); }
    }

    public void SerializeLib()
    {
        string json = JsonConvert.SerializeObject(SerializedLocations, Formatting.Indented);

        File.WriteAllText(@"CommandLib.json", json.ToString());
    }

    public Dictionary<DeviceIdentCmd, DeviceCmd> DeserializeLib()
    {
        var json =
            JsonConvert.DeserializeObject<List<KeyValuePair<DeviceIdentCmd, DeviceCmd>>>(
                File.ReadAllText(@"CommandLib.json"));

        Dictionary<DeviceIdentCmd, DeviceCmd> temp = new Dictionary<DeviceIdentCmd, DeviceCmd>();
        foreach (var cmd in json)
        {
            temp.Add(cmd.Key, cmd.Value);
        }

        return temp;
    }
    
    
    public void SerializeDevices(List<BaseDevice> devices)
    {
        var json = JsonConvert.SerializeObject(devices, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        File.WriteAllText(@"Devices.json", json.ToString());
    }

    public List<BaseDevice> DeserializeDevices()
    {
        var json =
            JsonConvert.DeserializeObject<List<BaseDevice>>(
                File.ReadAllText(@"Devices.json"), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
        return json;
    }
}