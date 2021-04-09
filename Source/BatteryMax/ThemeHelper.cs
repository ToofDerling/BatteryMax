using Microsoft.Win32;

namespace BatteryMax
{
    public enum WindowsTheme
    {
        Light,
        Dark
    }

    public class ThemeHelper
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "SystemUsesLightTheme";
        
        public static WindowsTheme GetWindowsTheme()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            
            var registryValueObject = key?.GetValue(RegistryValueName);
            if (registryValueObject == null)
            {
                Log.Write("SystemUsesLightTheme is null. Assuming default light theme");
                return WindowsTheme.Light;
            }

            var registryValue = (int)registryValueObject;

            return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
        }
    }
}
