using System;
using System.IO;

namespace SimilarCode.Load
{
    public class TemporaryFile : IDisposable
    {
        private bool disposedValue;
        private static readonly string SystemTemporaryDirectory = @"N:\"; //System.IO.Path.GetTempPath();
        public string Path { get; }
        public TemporaryFile(string contents)
        {
            // Path.GetTempFileName uses 7% of the execution time (Interop.Kernel32.GetTempFileNameW)
            Path = System.IO.Path.Join(SystemTemporaryDirectory, System.IO.Path.GetRandomFileName());
            File.WriteAllText(Path, contents);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        // Windows's internal delete functions are much faster than .NET's managed ones
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            Utilities.DeleteFile(Path);
                        }
                        else
                        {
                            File.Delete(Path);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}