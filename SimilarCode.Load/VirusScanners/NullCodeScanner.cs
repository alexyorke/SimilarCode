namespace SimilarCode.Load.VirusScanners
{
    internal class NullCodeScanner : IMaliciousSoftwareScanner
    {
        public bool IsMalicious(string toScan, string? fileName = null)
        {
            return false;
        }

        public void Dispose()
        {
            // no resources to dispose
        }
    }
}