using System.Runtime.InteropServices;
namespace SimilarCode.Load
{
    internal static class Utilities
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string lpFileName);
    }
}