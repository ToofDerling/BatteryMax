using BatteryMax.Properties;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Power;
using Windows.UI.ViewManagement;

namespace BatteryMax
{
    public class BatteryIconManager
    {
        public event EventHandler<EventArgs> BatteryChanged;

        public string UpdateText { get; private set; }

        public string WarningText { get; private set; }

        public Icon UpdateIcon { get; private set; }

        // The actual hardware battery 
        private Battery battery;

        private BatteryData batteryData;

        private BatteryData testBatteryData;

        private UISettings uiSettings;

        private WindowsTheme windowsTheme;

        public async Task InitializeDataAsync(BatteryData testBatteryData = null)
        {
            this.testBatteryData = testBatteryData;

            if (this.testBatteryData == null)
            {
                var device = new BatteryDevice();
                battery = await device.GetBatteryAsync();

                if (battery != null)
                {
                    // Based on observation this fires immediately when power status changes. But when unchanged it
                    // fires in 3-6 minutes intervals (which is useless of course).                  
                    battery.ReportUpdated += (s, e) => OnBatteryDataChanged();
                }
            }

            uiSettings = new UISettings();
            // Based on observation this fires immediately when Windows theme changes. But none of colors in GetColorValue()
            // or UIElementColor() are actually updated, so changing the colors must be done manually.
            uiSettings.ColorValuesChanged += (s, e) => OnWindowsThemeChanged();

            windowsTheme = ThemeHelper.GetWindowsTheme();
        }

        private void OnBatteryDataChanged()
        {
            Log.Write("OnBatteryDataChanged");
            OnDataChanged();
        }

        private void OnWindowsThemeChanged()
        {
            Log.Write($"OnWindowsThemeChanged -> {ThemeHelper.GetWindowsTheme()}");
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            if (!stopThreadLoop)
            {
                lock (lockObj)
                {
                    Monitor.Pulse(lockObj);
                }
            }
        }

        public void Start()
        {
            var managerThread = new Thread(ThreadLoop)
            {
                IsBackground = true
            };

            managerThread.Start();
        }

        public void Stop()
        {
            stopThreadLoop = true;

            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }
        }

        private bool Update()
        {
            var currentBatteryData = testBatteryData ?? new BatteryData(battery);
            var currentWindowsTheme = ThemeHelper.GetWindowsTheme();

            var windowsThemeChanged = windowsTheme != currentWindowsTheme;

            if (windowsThemeChanged
                || batteryData == null
                || batteryData.CurrentCharge != currentBatteryData.CurrentCharge
                || batteryData.CurrentTime != currentBatteryData.CurrentTime
                || batteryData.IsCharging != currentBatteryData.IsCharging
                || batteryData.IsPluggedInNotCharging != currentBatteryData.IsPluggedInNotCharging)
            {
                if (currentBatteryData.IsNotAvailable)
                {
                    CreateBatteryUpdateText(Resources.BatteryNotFound);
                    CreateBatteryWarningText(Resources.BatteryNotFound);

                    CreateBatteryIcon(currentBatteryData, false, currentWindowsTheme, windowsThemeChanged);
                }
                else
                {
                    Log.Write(currentBatteryData.ToString());

                    var currentUpdateText = TextFormatter.FormatBatteryUpdateText(currentBatteryData);
                    CreateBatteryUpdateText(currentUpdateText);
                    WarningText = null;

                    var chargingChanged = batteryData == null || batteryData.IsCharging != currentBatteryData.IsCharging;
                    CreateBatteryIcon(currentBatteryData, chargingChanged, currentWindowsTheme, windowsThemeChanged);
                }

                batteryData = currentBatteryData;
                windowsTheme = currentWindowsTheme;
                return true;
            }
            return false;
        }

        private string warningText = null;

        private void CreateBatteryWarningText(string currentWarningText)
        {
            if (warningText != currentWarningText)
            {
                WarningText = warningText = currentWarningText;
            }
            else
            {
                WarningText = null;
            }
        }

        private string updateText = null;

        private void CreateBatteryUpdateText(string currentUpdateText)
        {
            if (updateText != currentUpdateText)
            {
                UpdateText = updateText = currentUpdateText;
            }
            else
            {
                UpdateText = null;
            }
        }

        private readonly object lockObj = new();

        private bool stopThreadLoop;

        private void ThreadLoop()
        {
            try
            {
                while (!stopThreadLoop)
                {
                    if (Update() && !stopThreadLoop)
                    {
                        BatteryChanged?.Invoke(this, new EventArgs());
                    }

                    if (!stopThreadLoop)
                    {
                        lock (lockObj)
                        {
                            var timedOut = !Monitor.Wait(lockObj, 1000);
                            Log.Write($"timedOut={timedOut}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write($"BatteryIconManager thread error {ex}");
            }
        }

        private int drawWidth = -1;

        private void CreateBatteryIcon(BatteryData battery, bool chargingChanged, WindowsTheme theme, bool themeChanged)
        {
            var iconSettings = IconSettings.GetSettings(theme);

            var builder = new IconBuilder(iconSettings);

            var currentDrawWidth = builder.GetDrawingWidth(battery);

            var buildIcon = currentDrawWidth != drawWidth || chargingChanged || themeChanged;
            drawWidth = currentDrawWidth;

            Log.Write($"{nameof(buildIcon)}={buildIcon} {nameof(currentDrawWidth)}={currentDrawWidth} {nameof(chargingChanged)}={chargingChanged} {nameof(themeChanged)}={themeChanged}  ");

            if (buildIcon)
            {
                UpdateIcon = builder.DrawIcon(battery, currentDrawWidth);
            }
            else
            {
                UpdateIcon = null;
            }
        }
    }
}
