using BatteryMax.Properties;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Threading;

namespace BatteryMax
{
    public class BatteryIconManager
    {
        public event EventHandler<EventArgs> BatteryChanged;

        public string UpdateText { get; private set; }

        public string WarningText { get; private set; }

        public Icon UpdateIcon { get; private set; }

        public BatteryIconManager()
        {
            Update();
        }

        public void Start()
        {
            var managerThread = new Thread(ThreadLööp)
            {
                IsBackground = true
            };

            managerThread.Start();

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.StatusChange && !stop)
            {
                lock (lockObj)
                {
                    Monitor.Pulse(lockObj);
                }

                Log.Write($"Handled {nameof(PowerModes.StatusChange)}");
            }
        }

        private bool stop;

        public void Stop()
        {
            stop = true;

            // MSDN: "Because this is a static event, you must detach your event handlers when your application is disposed, or memory leaks will result."
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;

            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }
        }

        private Battery currentBattery = null;

        private bool Update()
        {
            var battery = new Battery();

            if (currentBattery == null
                || currentBattery.CurrentCharge != battery.CurrentCharge
                || currentBattery.CurrentTime != battery.CurrentTime
                || currentBattery.IsCharging != battery.IsCharging
                || currentBattery.IsPluggedInNotCharging != battery.IsPluggedInNotCharging)
            {
                if (battery.IsNotAvailable)
                {
                    CreateBatteryText(Resources.BatteryNotFound);
                    CreateBatteryWarningText(Resources.BatteryNotFound);

                    CreateBatteryIcon(battery, false);
                }
                else
                {
                    Log.Write(battery.ToString());

                    var updateText = FormatBatteryUpdateText(battery);
                    CreateBatteryText(updateText);
                    WarningText = null;

                    var chargingChanged = currentBattery == null || currentBattery.IsCharging != battery.IsCharging;
                    CreateBatteryIcon(battery, chargingChanged);
                }

                currentBattery = battery;
                return true;
            }
            return false;
        }

        private string currentWarningText = null;

        private void CreateBatteryWarningText(string warning)
        {
            if (currentWarningText != warning)
            {
                WarningText = currentWarningText = warning;
            }
            else
            {
                WarningText = null;
            }
        }

        private string currentUpdateText = null;

        private void CreateBatteryText(string updateText)
        {
            if (currentUpdateText != updateText)
            {
                UpdateText = currentUpdateText = updateText;
            }
            else
            {
                UpdateText = null;
            }
        }

        private static string FormatBatteryUpdateText(Battery battery)
        {
            var charge = battery.CurrentCharge;

            if (battery.IsCharging)
            {
                if (battery.IsAboveMaximumCharge)
                {
                    return string.Format(Resources.ChargingAboveMaximum, charge, Settings.MaximumCharge);
                }
                if (battery.CurrentTime.TotalSeconds == 0)
                {
                    return string.Format(Resources.Charging, charge);
                }
                if (battery.CurrentTime.Hours > 0 && battery.CurrentTime.Minutes > 0)
                {
                    return string.Format(Resources.ChargingHourMin, charge, battery.CurrentTime.Hours, battery.CurrentTime.Minutes, Settings.MaximumCharge);
                }
                if (battery.CurrentTime.Hours > 0)
                {
                    return string.Format(Resources.ChargingHour, charge, battery.CurrentTime.Hours, Settings.MaximumCharge);
                }
                // This can return "0 min" and that's fine
                return string.Format(Resources.ChargingMin, charge, battery.CurrentTime.Minutes, Settings.MaximumCharge);
            }

            if (battery.IsPluggedInNotCharging)
            {
                return string.Format(Resources.PluggedInNotCharging, charge);
            }
            if (battery.IsBelowMinimumCharge)
            {
                return string.Format(Resources.RemainingBelowMinimum, charge, Settings.MinimumCharge);
            }
            if (battery.CurrentTime.TotalSeconds == 0)
            {
                return string.Format(Resources.Remaining, charge);
            }
            if (battery.CurrentTime.Hours > 0 && battery.CurrentTime.Minutes > 0)
            {
                return string.Format(Resources.RemainingHourMin, charge, battery.CurrentTime.Hours, battery.CurrentTime.Minutes, Settings.MinimumCharge);
            }
            if (battery.CurrentTime.Hours > 0)
            {
                return string.Format(Resources.RemainingHour, charge, battery.CurrentTime.Hours, Settings.MinimumCharge);
            }
            // This can return "0 min" and that's fine
            return string.Format(Resources.RemainingMin, charge, battery.CurrentTime.Minutes, Settings.MinimumCharge);
        }

        private readonly object lockObj = new object();

        private void ThreadLööp()
        {
            try
            {
                while (!stop)
                {
                    if (Update() && !stop)
                    {
                        BatteryChanged?.Invoke(this, new EventArgs());
                    }

                    if (!stop)
                    {
                        lock (lockObj)
                        {
                            Monitor.Wait(lockObj, 1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write($"BatteryIconManager thread error {ex}");
            }
        }

        private int currentDrawWidth = -1;

        private void CreateBatteryIcon(Battery battery, bool chargingChanged)
        {
            var iconSettings = IconSettings.GetSettings();

            var builder = new IconBuilder(iconSettings);

            var drawWidth = builder.GetDrawingWidth(battery);

            var buildIcon = drawWidth != currentDrawWidth || chargingChanged;
            currentDrawWidth = drawWidth;

            Log.Write($"{nameof(buildIcon)}={buildIcon} {nameof(chargingChanged)}={chargingChanged} {nameof(drawWidth)}={drawWidth}");

            if (buildIcon)
            {
                UpdateIcon = builder.BuildIcon(battery, drawWidth);
            }
            else
            {
                UpdateIcon = null;
            }
        }
    }
}
