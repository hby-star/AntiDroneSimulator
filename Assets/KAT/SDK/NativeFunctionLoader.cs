using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NativeFunctionLoader
{
    public NativeFunctionLoader(string fileName)
    {
        name = fileName;
    }

    string name = "";
    IntPtr handle = IntPtr.Zero;
    Dictionary<string, IntPtr> functionPointers = new Dictionary<string, IntPtr>();

    [DllImport("kernel32")]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    public T Invoke<T>(string function)
    {
        if (handle == IntPtr.Zero)
        {
#if UNITY_EDITOR_WIN
            var sdkFileName = Path.Combine(Application.dataPath, "KAT/SDK/Plugin/Win64/" + name);
#else
            var sdkFileName = Path.Combine(Application.dataPath, "Plugins/x86_64/" + name);
#endif

            Debug.Log("Going to LoadLibrary:" + sdkFileName);

            handle = LoadLibrary(sdkFileName);
            if (handle == IntPtr.Zero)
            {
                throw new Exception("Failed to load library " + sdkFileName);
            }
        }
        if (!functionPointers.ContainsKey(function))
        {
            IntPtr pointer = GetProcAddress(handle, function);
            if (pointer == IntPtr.Zero)
            {
                throw new Exception("Failed to get function pointer " + function);
            }
            functionPointers[function] = pointer;
        }

        return Marshal.GetDelegateForFunctionPointer<T>(functionPointers[function]);
    }

    public void Release()
    {
        if (handle != IntPtr.Zero)
        {
             foreach (System.Diagnostics.ProcessModule mod in System.Diagnostics.Process.GetCurrentProcess().Modules)
             {
                 if (mod.ModuleName.EndsWith(name))
                 {
                     Debug.Log("Free Library:" + name);
                     FreeLibrary(mod.BaseAddress);
                 }
             }
          
            handle = IntPtr.Zero;
            functionPointers.Clear();
        }

     
    }
}

