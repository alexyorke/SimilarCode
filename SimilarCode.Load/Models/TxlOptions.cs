using System;

namespace SimilarCode.Load.Models
{
    public class TxlOptions
    {
        public TxlOptions(string PathToTxl, string GrammarsFolder)
        {
            // choose default TXL location if not specified
            if (string.IsNullOrWhiteSpace(PathToTxl) && OperatingSystem.IsWindows())
            {
                PathToTxl = "txl.exe";
            }
            else if (string.IsNullOrWhiteSpace(PathToTxl) && (OperatingSystem.IsLinux() ||
                                                              OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst()))
            {
                PathToTxl = "txl";
            }

            this.PathToTxl = PathToTxl;
            this.GrammarsFolder = GrammarsFolder;
        }
        public string PathToTxl { get; }
        public string GrammarsFolder { get; }
    }
}