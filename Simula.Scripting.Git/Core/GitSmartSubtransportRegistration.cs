﻿using System;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GitSmartSubtransportRegistration
    {
        public IntPtr SubtransportCallback;
        public uint Rpc;
        public uint Param;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int create_callback(
            out IntPtr subtransport,
            IntPtr owner,
            IntPtr param);
    }
}
