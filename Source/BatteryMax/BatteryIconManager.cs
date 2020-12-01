using BatteryMax.Properties;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Text;
using System.Threading;

namespace BatteryMax
{
    public class BatteryIconManager
    {
        public event EventHandler<EventArgs> BatteryChanged;

        public string UpdateText { get; private set; }

        public string WarningText { get; private set; }

        public Icon UpdateIcon { get; private set; }

        private Battery TestBattery { get; set; }

        public BatteryIconManager(Battery testBattery = null)
        {
            TestBattery = testBattery;

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
            var battery = TestBattery ?? new Battery();

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

                    var updateText = TextFormatter.FormatBatteryUpdateText(battery);
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
                var image = builder.DrawImage(battery, drawWidth);
                UpdateIcon = builder.CreateIcon(image);
            }
            else
            {
                UpdateIcon = null;
            }
        }
    }
}
