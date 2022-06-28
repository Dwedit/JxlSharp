using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

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
    using JxlParallelRunInit = IntPtr;
    using JxlParallelRetCode = Int32;
    using JxlParallelRunFunction = IntPtr;

    internal unsafe partial class UnsafeNativeJXL
    {
        ///// <summary>
        ///// Parallel runner internally using std::thread. Use as JxlParallelRunner.
        ///// </summary>
        //internal static extern JxlParallelRetCode JxlThreadParallelRunner(
        //    void* runner_opaque, void* jpegxl_opaque, JxlParallelRunInit init,
        //    JxlParallelRunFunction func, uint32_t start_range, uint32_t end_range);

        /// <summary>
        /// Creates the runner for JxlThreadParallelRunner. Use as the opaque
        /// runner.
        /// </summary>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void* JxlThreadParallelRunnerCreate(
            [In] JxlMemoryManager* memory_manager, size_t num_worker_threads);

        /// <summary>
        /// Destroys the runner created by JxlThreadParallelRunnerCreate.
        /// </summary>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlThreadParallelRunnerDestroy(void* runner_opaque);

        /// <summary>
        /// Returns a default num_worker_threads value for
        /// JxlThreadParallelRunnerCreate.
        /// </summary>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern size_t JxlThreadParallelRunnerDefaultNumWorkerThreads();


    }
}
