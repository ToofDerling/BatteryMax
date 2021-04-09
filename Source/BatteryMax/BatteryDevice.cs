using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Power;

namespace BatteryMax
{
    public class BatteryDevice
    {
        public async Task<Battery> GetBatteryAsync()
        {
            var batterySelector = Battery.GetDeviceSelector();

            var batteryCollection = await DeviceInformation.FindAllAsync(batterySelector);
            Log.Write("Battery count {0}", batteryCollection.Count);

            if (batteryCollection.Count == 0)
            {
                return null;
            }

            var device0 = batteryCollection[0];

            var battery = await Battery.FromIdAsync(device0.Id);
            return battery;
        }
    }
}
