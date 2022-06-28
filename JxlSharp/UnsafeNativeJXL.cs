using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace JxlSharp
{
    //We declare a type to equal itself because of intellisense
    //otherwise intellisense would display the basic type under a different name
    //example: All "int" types would suddenly be displayed as JXL_BOOL if we didn't do this
    using UInt32 = UInt32;
    using Int32 = Int32;
    using IntPtr = IntPtr;
    using UIntPtr = UIntPtr;
    using Byte = Byte;
    using UInt64 = UInt64;

    //typedefs for C types
    using int32_t = Int32;
    using uint32_t = UInt32;
    using uint8_t = Byte;
    using size_t = UIntPtr;
    using JXL_BOOL = Int32;
    using uint64_t = UInt64;
    //"JxlParallelRunner" is a function pointer type
    using JxlParallelRunner = IntPtr;

    internal unsafe partial class UnsafeNativeJXL
    {
        internal static readonly IntPtr libJxlModule;
        internal static readonly IntPtr JxlThreadParallelRunner;

        private static class UnsafeNativeWin32
        {
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryW(string fileName);
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        }

        static IntPtr TryLoadLibJXL(string arch)
        {
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyPath = Path.GetDirectoryName(executingAssembly.Location);
            string libPath = Path.Combine(assemblyPath, "lib");
            string archPath = Path.Combine(libPath, arch);
            string dllName = Path.Combine(archPath, "libjxl.dll");
            if (!File.Exists(dllName))
            {
                archPath = Path.Combine(assemblyPath, arch);
                dllName = Path.Combine(archPath, "libjxl.dll");
            }
            if (!File.Exists(dllName))
            {
                dllName = Path.Combine(assemblyPath, "libjxl.dll");
            }

            if (File.Exists(dllName))
            {
                return UnsafeNativeWin32.LoadLibraryW(dllName);
            }
            else
            {
                return IntPtr.Zero;
            }
        }


        /// <summary>
        /// Loads the correct version of "libjxl.dll" for the executing architecture, this allows the DllImports to work
        /// </summary>
        static UnsafeNativeJXL()
        {
            string arch = "x64";

            if (Marshal.SizeOf(typeof(IntPtr)) == 8)
            {
                arch = "x64";
            }
            else if (Marshal.SizeOf(typeof(IntPtr)) == 4)
            {
                arch = "x86";
            }
            libJxlModule = TryLoadLibJXL(arch);
            if (libJxlModule == IntPtr.Zero)
            {
                throw new DllNotFoundException("Failed to load LibJxl.dll - Check that this file is not missing");
            }
            JxlThreadParallelRunner = UnsafeNativeWin32.GetProcAddress(libJxlModule, "JxlThreadParallelRunner");
            if (JxlThreadParallelRunner == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException("Failed to find JxlThreadParallelRunner in LibJxl.dll, the DLL is a bad file");
            }
        }




        /// <summary>
        /// Dummy opaque struct, pass "null" instead of using it
        /// </summary>
        internal struct JxlMemoryManager
        {

        }
    }
}
