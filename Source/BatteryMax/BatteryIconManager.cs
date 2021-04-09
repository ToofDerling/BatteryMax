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
        private WindowsTheme currentWindowsTheme;

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
                    battery.ReportUpdated += (s, e) => OnBatteryReportUpdated();
                }
            }

            InitializeTheme();
        }

        private void InitializeTheme()
        {
            windowsTheme = ThemeHelper.GetWindowsTheme();
            currentWindowsTheme = windowsTheme;

            uiSettings = new UISettings();
            // Based on observation this fires immediately when Windows theme changes. But none of colors in GetColorValue()
            // or UIElementColor() are actually updated, so changing the colors must be done manually.
            uiSettings.ColorValuesChanged += (s, e) => OnColorValuesChanged();
        }

        private void OnBatteryReportUpdated()
        {
            SignalDataChanged();
        }

        private void OnColorValuesChanged()
        {
            currentWindowsTheme = ThemeHelper.GetWindowsTheme();
            if (windowsTheme != currentWindowsTheme)
            {
                Log.Write($"OnWindowsThemeChanged -> {currentWindowsTheme}");
                SignalDataChanged();
            }
        }

        private void SignalDataChanged()
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
            var currentBatteryData = testBatteryData != null ? testBatteryData.GetNextTestData() : new BatteryData(battery);

            // These conditions all forces icon (re)drawing, even if there's no change in CurrentCharge or the change is too small
            // to cause the number of levels to update.
            var drawIcon = windowsTheme != currentWindowsTheme
                || batteryData == null
                || batteryData.IsCharging != currentBatteryData.IsCharging
                || batteryData.IsAboveMaximumCharge != currentBatteryData.IsAboveMaximumCharge
                || batteryData.IsBelowMinimumCharge != currentBatteryData.IsBelowMinimumCharge
                || batteryData.IsCriticalCharge != currentBatteryData.IsCriticalCharge;

            if (drawIcon
                || batteryData.CurrentCharge != currentBatteryData.CurrentCharge
                || batteryData.CurrentTime != currentBatteryData.CurrentTime
                || batteryData.IsPluggedInNotCharging != currentBatteryData.IsPluggedInNotCharging)
            {
                if (currentBatteryData.IsNotAvailable)
                {
                    CreateBatteryUpdateText(Resources.BatteryNotFound);
                    CreateBatteryWarningText(Resources.BatteryNotFound);

                    CreateBatteryIcon(currentBatteryData, currentWindowsTheme, drawIcon);
                }
                else
                {
                    Log.Write(currentBatteryData.ToString());

                    var currentUpdateText = TextFormatter.FormatBatteryUpdateText(currentBatteryData);
                    CreateBatteryUpdateText(currentUpdateText);
                    WarningText = null;

                    CreateBatteryIcon(currentBatteryData, currentWindowsTheme, drawIcon);
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

        private int drawWidth = -1;

        private void CreateBatteryIcon(BatteryData battery, WindowsTheme theme, bool drawIcon)
        {
            var iconSettings = IconSettings.GetSettings(theme);

            var builder = new IconBuilder(iconSettings);

            var currentDrawWidth = builder.GetDrawingWidth(battery);

            var buildIcon = drawIcon || currentDrawWidth != drawWidth;

            if (buildIcon)
            {
                UpdateIcon = builder.DrawIcon(battery, currentDrawWidth);
            }
            else
            {
                UpdateIcon = null;
            }

            drawWidth = currentDrawWidth;
        }
    }
}
