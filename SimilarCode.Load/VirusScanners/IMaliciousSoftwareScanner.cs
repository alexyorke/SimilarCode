namespace SimilarCode.Load.VirusScanners
{
    internal interface IMaliciousSoftwareScanner
    {
        public bool IsMalicious(string toScan, string? fileName = null);
        void Dispose();
    }
}