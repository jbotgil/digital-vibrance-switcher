using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DigitalVibrance.Native
{
    static class NativeLibrary
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr LoadLibraryW(string path);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr h, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr h);

        public static IntPtr Load(string path)
        {
            IntPtr h = LoadLibraryW(path);
            if (h == IntPtr.Zero)
                throw new InvalidOperationException(string.Format("Cannot load {0}", path));
            return h;
        }

        public static IntPtr GetExport(IntPtr hModule, string name)
        {
            IntPtr p = GetProcAddress(hModule, name);
            if (p == IntPtr.Zero)
                throw new InvalidOperationException(string.Format("Export '{0}' not found", name));
            return p;
        }

        public static void Free(IntPtr hModule)
        {
            if (hModule != IntPtr.Zero)
                FreeLibrary(hModule);
        }
    }

    class NvDisplayInfo
    {
        public uint Handle { get; set; }
        public int Current { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Default { get; set; }
    }

    static class NvApi
    {
        const string DllName = "nvapi64.dll";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr QueryInterfaceDelegate(uint id);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int InitDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int EnumDisplayDelegate(uint index, out uint handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int GetDvcExDelegate(uint handle, uint outputId, ref DvcEx info);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int SetDvcExDelegate(uint handle, uint outputId, ref DvcEx info);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        struct DvcEx
        {
            public uint Version;
            public int Current;
            public int Min;
            public int Max;
            public int Default;
        }

        static IntPtr _moduleHandle;
        static QueryInterfaceDelegate _queryInterface;
        static InitDelegate _init;
        static EnumDisplayDelegate _enumDisplay;
        static GetDvcExDelegate _getDvcEx;
        static SetDvcExDelegate _setDvcEx;
        static List<NvDisplayInfo> _displays;

        public static IReadOnlyList<NvDisplayInfo> Displays
        {
            get { return _displays; }
        }

        public static string LastErrorMessage
        {
            get; private set;
        }

        static T GetFunction<T>(uint id) where T : class
        {
            IntPtr ptr = _queryInterface(id);
            if (ptr == IntPtr.Zero) return null;
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
        }

        public static bool Initialize()
        {
            try
            {
                _moduleHandle = NativeLibrary.Load(DllName);

                _queryInterface = Marshal.GetDelegateForFunctionPointer(
                    NativeLibrary.GetExport(_moduleHandle, "nvapi_QueryInterface"),
                    typeof(QueryInterfaceDelegate)) as QueryInterfaceDelegate;

                _init = GetFunction<InitDelegate>(0x0150E828u);
                _enumDisplay = GetFunction<EnumDisplayDelegate>(0x9ABDD40Du);
                _getDvcEx = GetFunction<GetDvcExDelegate>(0x0E45002Du);
                _setDvcEx = GetFunction<SetDvcExDelegate>(0x4A82C2B1u);

                if (_init == null || _enumDisplay == null || _getDvcEx == null || _setDvcEx == null)
                {
                    LastErrorMessage = "NVAPI functions not available. Unsupported driver version.";
                    Shutdown();
                    return false;
                }

                int result = _init();
                if (result != 0)
                {
                    LastErrorMessage = string.Format("NvAPI_Initialize returned 0x{0:X8}", result);
                    Shutdown();
                    return false;
                }

                _displays = new List<NvDisplayInfo>();
                for (uint i = 0; i < 16; i++)
                {
                    uint handle;
                    if (_enumDisplay(i, out handle) != 0) break;

                    var info = new DvcEx { Version = 0x10014 };
                    if (_getDvcEx(handle, 0, ref info) == 0)
                    {
                        _displays.Add(new NvDisplayInfo
                        {
                            Handle = handle,
                            Current = info.Current,
                            Min = info.Min,
                            Max = info.Max,
                            Default = info.Default
                        });
                    }
                }

                if (_displays.Count == 0)
                {
                    LastErrorMessage = "No NVIDIA displays found.";
                    Shutdown();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                Shutdown();
                return false;
            }
        }

        static void RefreshDisplays()
        {
            if (_displays == null) return;

            foreach (NvDisplayInfo disp in _displays)
            {
                var info = new DvcEx { Version = 0x10014 };
                if (_getDvcEx(disp.Handle, 0, ref info) == 0)
                {
                    disp.Current = info.Current;
                    disp.Min = info.Min;
                    disp.Max = info.Max;
                    disp.Default = info.Default;
                }
            }
        }

        public static bool SetVibrance(int level)
        {
            if (_displays == null) return false;

            level = Math.Max(0, Math.Min(100, level));

            foreach (NvDisplayInfo disp in _displays)
            {
                var info = new DvcEx
                {
                    Version = 0x10014,
                    Current = level,
                    Min = disp.Min,
                    Max = disp.Max,
                    Default = disp.Default
                };
                _setDvcEx(disp.Handle, 0, ref info);
            }

            RefreshDisplays();
            return true;
        }

        public static int GetCurrentVibrance()
        {
            if (_displays == null || _displays.Count == 0) return 0;
            return _displays[0].Current;
        }

        public static void Shutdown()
        {
            _moduleHandle = IntPtr.Zero;
            _displays = null;
        }

        public static bool IsAvailable
        {
            get { return _displays != null && _displays.Count > 0; }
        }
    }
}