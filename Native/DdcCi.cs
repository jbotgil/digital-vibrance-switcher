using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DigitalVibrance.Native
{
    static class DdcCi
    {
        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetMonitorCapabilities(IntPtr hMonitor, out uint pdwMonitorCapabilities, out uint pdwSupportedColorTemperatures);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetMonitorContrast(IntPtr hMonitor, out uint pdwMinimumContrast, out uint pdwCurrentContrast, out uint pdwMaximumContrast);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        const uint MONITOR_DEFAULTTOPRIMARY = 1;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        public static bool SupportsBrightness(IntPtr monitorHandle)
        {
            if (monitorHandle == IntPtr.Zero) return false;
            uint caps, temps;
            return GetMonitorCapabilities(monitorHandle, out caps, out temps);
        }

        public static int GetBrightness(IntPtr monitorHandle)
        {
            if (monitorHandle == IntPtr.Zero) return -1;
            uint min, cur, max;
            if (GetMonitorBrightness(monitorHandle, out min, out cur, out max))
                return (int)cur;
            return -1;
        }

        public static bool SetBrightness(IntPtr monitorHandle, int level)
        {
            if (monitorHandle == IntPtr.Zero) return false;
            uint min, cur, max;
            if (!GetMonitorBrightness(monitorHandle, out min, out cur, out max))
                return false;
            level = Math.Max((int)min, Math.Min((int)max, level));
            return SetMonitorBrightness(monitorHandle, (uint)level);
        }

        public static int GetContrast(IntPtr monitorHandle)
        {
            if (monitorHandle == IntPtr.Zero) return -1;
            uint min, cur, max;
            if (GetMonitorContrast(monitorHandle, out min, out cur, out max))
                return (int)cur;
            return -1;
        }

        public static bool SetContrast(IntPtr monitorHandle, int level)
        {
            if (monitorHandle == IntPtr.Zero) return false;
            uint min, cur, max;
            if (!GetMonitorContrast(monitorHandle, out min, out cur, out max))
                return false;
            level = Math.Max((int)min, Math.Min((int)max, level));
            return SetMonitorContrast(monitorHandle, (uint)level);
        }

        static bool EnumMonitors(IntPtr hPhysicalMonitor)
        {
            if (hPhysicalMonitor == IntPtr.Zero) return false;
            uint caps, temps;
            return GetMonitorCapabilities(hPhysicalMonitor, out caps, out temps);
        }
    }
}