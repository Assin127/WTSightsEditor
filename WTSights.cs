using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WTSightsEditor
{
    public class WTSights
    {
        /// <summary>
        /// Вычисляет процент part от full
        /// </summary>
        /// <param name="full"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        public static int GetPercent(int full, int part)
        {
            double percent = (double)((100 * (part)) / ((double)full));
            if (percent > 100) percent = 100;
            return (int)percent;
        }


        public static int GetHeight()
        {
            int DPI = Int32.Parse((string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager", "LastLoadedDPI", "96"));
            return (int)(System.Windows.SystemParameters.PrimaryScreenHeight* DPI / 96);
        }

        public static int GetWidth()
        {
            int DPI = Int32.Parse((string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager", "LastLoadedDPI", "96"));
            return (int)(System.Windows.SystemParameters.PrimaryScreenWidth * DPI / 96);
        }
    }
}
