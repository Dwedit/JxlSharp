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
        internal static readonly IntPtr _libJxlThreadModule;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate void* JxlThreadParallelRunnerCreate_FUNC(JxlMemoryManager* memoryManager, UIntPtr num_worker_threads);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate void JxlThreadParallelRunnerDestroy_FUNC(void* runner_opaque);
        internal static readonly JxlThreadParallelRunnerCreate_FUNC JxlThreadParallelRunnerCreate;
        internal static readonly JxlThreadParallelRunnerDestroy_FUNC JxlThreadParallelRunnerDestroy;

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

        /// <summary>
        /// Loads the correct version of "jxl.dll" for the executing architecture, this allows the DllImports to work
        /// </summary>
        static UnsafeNativeJxl()
        {
            string dllName = "jxl.dll";
            string dllName2 = "jxl_threads.dll";

            string dllDependency1 = "brotlicommon.dll";
            string dllDependency2 = "brotlidec.dll";
            string dllDependency3 = "brotlienc.dll";

            IntPtr module1 = TryLoadLibrary(dllDependency1);
            IntPtr module2 = TryLoadLibrary(dllDependency2);
            IntPtr module3 = TryLoadLibrary(dllDependency3);

            libJxlModule = TryLoadLibrary(dllName);
            if (libJxlModule == IntPtr.Zero)
            {
                throw new DllNotFoundException("Failed to load " + dllName + " - Check that this file is not missing");
            }
            IntPtr jxlThreadParallelRunnerCreate = UnsafeNativeWin32.GetProcAddress(libJxlModule, "JxlThreadParallelRunnerCreate");
            IntPtr jxlThreadParallelRunnerDestroy = UnsafeNativeWin32.GetProcAddress(libJxlModule, "JxlThreadParallelRunnerDestroy");
            JxlThreadParallelRunner = UnsafeNativeWin32.GetProcAddress(libJxlModule, "JxlThreadParallelRunner");
            if (jxlThreadParallelRunnerCreate == IntPtr.Zero ||
                jxlThreadParallelRunnerDestroy == IntPtr.Zero ||
                JxlThreadParallelRunner == IntPtr.Zero)
            {
                _libJxlThreadModule = TryLoadLibrary(dllName2);
                if (_libJxlThreadModule != IntPtr.Zero)
                {
                    jxlThreadParallelRunnerCreate = UnsafeNativeWin32.GetProcAddress(_libJxlThreadModule, "JxlThreadParallelRunnerCreate");
                    jxlThreadParallelRunnerDestroy = UnsafeNativeWin32.GetProcAddress(_libJxlThreadModule, "JxlThreadParallelRunnerDestroy");
                    JxlThreadParallelRunner = UnsafeNativeWin32.GetProcAddress(_libJxlThreadModule, "JxlThreadParallelRunner");
                }
                if (jxlThreadParallelRunnerCreate == IntPtr.Zero ||
                    jxlThreadParallelRunnerDestroy == IntPtr.Zero ||
                    JxlThreadParallelRunner == IntPtr.Zero)
                {
                    throw new EntryPointNotFoundException("Failed to find one of 'JxlThreadParallelRunnerCreate', 'JxlThreadParallelRunnerDestroy', or 'JxlThreadParallelRunner' in " + dllName + " or " + dllName2 + ", check if the DLL is okay");
                }
            }
            JxlThreadParallelRunnerCreate = (JxlThreadParallelRunnerCreate_FUNC)Marshal.GetDelegateForFunctionPointer(jxlThreadParallelRunnerCreate, typeof(JxlThreadParallelRunnerCreate_FUNC));
            JxlThreadParallelRunnerDestroy = (JxlThreadParallelRunnerDestroy_FUNC)Marshal.GetDelegateForFunctionPointer(jxlThreadParallelRunnerDestroy, typeof(JxlThreadParallelRunnerDestroy_FUNC));
        }
    }
}