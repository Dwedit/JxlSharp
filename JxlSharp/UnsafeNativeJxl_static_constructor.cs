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

    internal unsafe partial class UnsafeNativeJxl
    {
        internal static readonly IntPtr libJxlModule;
        internal static readonly IntPtr JxlThreadParallelRunner;
        internal static readonly IntPtr libJxlThreadModule;
        internal static readonly IntPtr libJxlCmsModule;
        internal static readonly IntPtr brotliCommonModule;
        internal static readonly IntPtr brotliDecModule;
        internal static readonly IntPtr brotliEncModule;

        private static class UnsafeNativeWin32
        {
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryW(string fileName);
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        }

        static IntPtr TryLoadLibrary(string arch, string inputDllName)
        {
            if (String.IsNullOrEmpty(arch))
            {
                arch = "x64";
                if (Marshal.SizeOf(typeof(IntPtr)) == 8)
                {
                    arch = "x64";
                }
                else if (Marshal.SizeOf(typeof(IntPtr)) == 4)
                {
                    arch = "x86";
                }
            }

            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyPath = Path.GetDirectoryName(executingAssembly.Location);
            string libPath = Path.Combine(assemblyPath, "lib");
            string archPath = Path.Combine(libPath, arch);
            string dllName = Path.Combine(archPath, inputDllName);
            if (!File.Exists(dllName))
            {
                archPath = Path.Combine(assemblyPath, arch);
                dllName = Path.Combine(archPath, inputDllName);
            }
            if (!File.Exists(dllName))
            {
                dllName = Path.Combine(assemblyPath, inputDllName);
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

        static IntPtr TryLoadLibrary(string inputDllName)
        {
            return TryLoadLibrary("", inputDllName);
        }

        static IntPtr TryLoadLibraryWithThrow(string inputDllName)
        {
            IntPtr module = TryLoadLibrary(inputDllName);
            if (module == IntPtr.Zero)
            {
                throw new DllNotFoundException("Failed to load " + inputDllName + " - Check that this file is not missing");
            }
            return module;
        }

        /// <summary>
        /// Loads the correct version of "jxl.dll" for the executing architecture, this allows the DllImports to work
        /// Also calls GetProcAddress for JxlThreadParallelRunner because we need a native pointer for that function
        /// </summary>
        static UnsafeNativeJxl()
        {
            //This function preloads the DLL files because otherwise it won't find them in the directory that contains them.
            const string jxlDllName = "jxl.dll";
            const string jxlThreadsDllName = "jxl_threads.dll";
            const string jxlCmsDllName = "jxl_cms.dll";
            const string brotliCommonDllName = "brotlicommon.dll";
            const string brotliDecDllName = "brotlidec.dll";
            const string brotliEncDllName = "brotlienc.dll";

            //must load brotli first
            brotliCommonModule = TryLoadLibraryWithThrow(brotliCommonDllName);
            brotliDecModule = TryLoadLibraryWithThrow(brotliDecDllName);
            brotliEncModule = TryLoadLibraryWithThrow(brotliEncDllName);
            //load jxl, and jxl_threads
            libJxlThreadModule = TryLoadLibraryWithThrow(jxlThreadsDllName);
            libJxlCmsModule = TryLoadLibraryWithThrow(jxlCmsDllName);
            libJxlModule = TryLoadLibraryWithThrow(jxlDllName);

            //Get function pointer for JxlThreadParallelRunner, so we can call JxlDecoderSetParallelRunner/JxlEncoderSetParallelRunner
            JxlThreadParallelRunner = UnsafeNativeWin32.GetProcAddress(libJxlThreadModule, "JxlThreadParallelRunner");
            if (JxlThreadParallelRunner == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException("Failed to find 'JxlThreadParallelRunner' in " + jxlThreadsDllName + ", check if the DLL is okay");
            }
        }
    }
}