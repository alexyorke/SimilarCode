using System;
using System.Runtime.InteropServices;
using SimilarCode.Load.VirusScanners;

namespace SimilarCode.Load
{
    internal class Amsi : IMaliciousSoftwareScanner, IDisposable
    {
        private static IntPtr _amsiContext;
        private static IntPtr _session;
        public Amsi()
        {
            int returnValue;

            returnValue = AmsiInitialize("SimilarCode", out _amsiContext);
            if (returnValue != (int)HRESULT.S_OK)
            {
                throw new InvalidOperationException($"AmsiInitialize failed: {returnValue}");
            }

            returnValue = AmsiOpenSession(_amsiContext, out _session);
            if (returnValue != (int)HRESULT.S_OK)
            {
                AmsiCloseSession(_amsiContext, _session);
                AmsiUninitialize(_amsiContext);
                throw new InvalidOperationException($"AmsiOpenSession failed: {returnValue}");
            }
        }
        public enum AMSI_RESULT
        {
            AMSI_RESULT_CLEAN = 0,
            AMSI_RESULT_NOT_DETECTED = 1,
            AMSI_RESULT_DETECTED = 32768
        }

        public enum HRESULT
        {
            S_OK = 0
        }

        [DllImport("Amsi.dll", EntryPoint = "AmsiInitialize", CallingConvention = CallingConvention.StdCall)]
        public static extern int AmsiInitialize([MarshalAs(UnmanagedType.LPWStr)] string appName, out IntPtr amsiContext);

        [DllImport("Amsi.dll", EntryPoint = "AmsiUninitialize", CallingConvention = CallingConvention.StdCall)]
        public static extern void AmsiUninitialize(IntPtr amsiContext);

        [DllImport("Amsi.dll", EntryPoint = "AmsiOpenSession", CallingConvention = CallingConvention.StdCall)]
        public static extern int AmsiOpenSession(IntPtr amsiContext, out IntPtr session);

        [DllImport("Amsi.dll", EntryPoint = "AmsiCloseSession", CallingConvention = CallingConvention.StdCall)]
        public static extern void AmsiCloseSession(IntPtr amsiContext, IntPtr session);

        [DllImport("Amsi.dll", EntryPoint = "AmsiScanString", CallingConvention = CallingConvention.StdCall)]
        public static extern int AmsiScanString(IntPtr amsiContext, [In()][MarshalAs(UnmanagedType.LPWStr)] string @string, [In()][MarshalAs(UnmanagedType.LPWStr)] string contentName, IntPtr session, out Amsi.AMSI_RESULT result);
        [DllImport("Amsi.dll", EntryPoint = "AmsiScanBuffer", CallingConvention = CallingConvention.StdCall)]
        public static extern int AmsiScanBuffer(IntPtr amsiContext, byte[] buffer, ulong length, string contentName, IntPtr session, out Amsi.AMSI_RESULT result);

        //This method apparently exists on MSDN but not in AMSI.dll (version 4.9.10586.0)   
        [DllImport("Amsi.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool AmsiResultIsMalware(Amsi.AMSI_RESULT result);

        public bool IsMalicious(string toScan, string? fileName = null)
        {
            if (string.IsNullOrEmpty(toScan)) return false;
            var amsiScanStringResult = AmsiScanString(_amsiContext, toScan, fileName ?? string.Empty, _session, out var result);

            if (amsiScanStringResult != (int)HRESULT.S_OK)
            {
                throw new InvalidOperationException("AmsiScanString failed: " + (int)amsiScanStringResult);
            }

            return result >= AMSI_RESULT.AMSI_RESULT_DETECTED;
        }

        private void ReleaseUnmanagedResources()
        {
            AmsiCloseSession(_amsiContext, _session);
            AmsiUninitialize(_amsiContext);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}