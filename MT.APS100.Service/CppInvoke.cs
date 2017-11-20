using System;
using System.Runtime.InteropServices;

namespace MT.APS100.Service
{
    public class CppInvoke
    {
        [DllImport(@"kernel32.dll", EntryPoint = "LoadLibrary")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport(@"kernel32.dll", EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetProcAddress(IntPtr lib, string funcName);

        [DllImport(@"kernel32.dll", EntryPoint = "FreeLibrary")]
        private static extern IntPtr FreeLibrary(IntPtr lib);

        private IntPtr hLib;

        public CppInvoke(string DLLPath)
        {
            hLib = LoadLibrary(DLLPath);
        }

        ~CppInvoke()
        {
            FreeLibrary(hLib);
        }

        //
        public Delegate Invoke(string APIName, Type t)
        {
            IntPtr api = GetProcAddress(hLib, APIName);
            return (Delegate)Marshal.GetDelegateForFunctionPointer(api, t);
        }
    }
}