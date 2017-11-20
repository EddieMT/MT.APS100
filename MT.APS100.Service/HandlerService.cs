using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MT.APS100.Service
{
    public class HandlerService
    {
        private CppInvoke dll;

        private delegate int FuncVoid();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FuncIntArray(int[] value);

        private FuncVoid loadFunction;
        private FuncVoid startFunction;
        private FuncIntArray eotFunction;
        private FuncVoid stopFunction;
        private FuncVoid unloadFunction;

        public HandlerService(string dLLPath)
        {
            if (!File.Exists(dLLPath))
            {
                throw new Exception("Handler dll is missing, please contact the developer!");
            }
            dll = new CppInvoke(dLLPath);
            loadFunction = (FuncVoid)dll.Invoke("Setup", typeof(FuncVoid));
            startFunction = (FuncVoid)dll.Invoke("Start", typeof(FuncVoid));
            eotFunction = (FuncIntArray)dll.Invoke("EOTProcess", typeof(FuncIntArray));
            stopFunction = (FuncVoid)dll.Invoke("Stop", typeof(FuncVoid));
            unloadFunction = (FuncVoid)dll.Invoke("Reset", typeof(FuncVoid));
        }

        public void Load()
        {
            loadFunction();
        }

        public void Start()
        {
            startFunction();
        }

        public void EOT(List<int> value)
        {
            eotFunction(value.ToArray());
        }

        public void Stop()
        {
            stopFunction();
        }

        public void Unload()
        {
            unloadFunction();
        }
    }
}