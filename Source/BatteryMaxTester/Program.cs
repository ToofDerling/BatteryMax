using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using Windows.UI.ViewManagement;

namespace BatteryMaxTester
{
    public class Program
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "SystemUsesLightTheme";

        private enum WindowsTheme
        {
            Light,
            Dark
        }
        private static WindowsTheme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }

        static void Main(string[] _)
        {
            Console.WriteLine(GetWindowsTheme());

            var settings = new UISettings();
            settings.ColorValuesChanged += Settings_ColorValuesChanged;

            Console.ReadLine();
        }
        private static void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            Console.WriteLine(GetWindowsTheme());
        }
    }
}
