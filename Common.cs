using Microsoft.Win32;
using System;

namespace WTSightsEditor
{
    internal class Common
    {
        public static int GetHeight()
        {
            int DPI = Int32.Parse((string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager", "LastLoadedDPI", "96"));
            return (int)(System.Windows.SystemParameters.PrimaryScreenHeight * DPI / 96);
        }

        public static int GetWidth()
        {
            int DPI = Int32.Parse((string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager", "LastLoadedDPI", "96"));
            return (int)(System.Windows.SystemParameters.PrimaryScreenWidth * DPI / 96);
        }
    }
}
