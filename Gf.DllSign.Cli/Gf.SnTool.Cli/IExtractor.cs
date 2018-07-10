using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gf.SnTool.Cli
{
    public interface IExtractor
    {
        void Extract(string compressedFile, string outputDirectory);
    }

    public class ZipExtractor : IExtractor
    {
        private const string ARCHIVE_FORMAT = ".zip";
        public void Extract(string compressedFile, string outputDirectory)
        {
            Log.Information("ExtractSolutionZipFile: [compressedFile: {0}; outputDirectory: {1}]", compressedFile, outputDirectory);
            if (!File.Exists(compressedFile))
            {
                throw new FileNotFoundException(string.Format("compressedFile:{0} is not found", compressedFile));
            }

            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new ArgumentException(string.Format("outputDirectory:{0} is invalid", outputDirectory));
            }

            string extension = Path.GetExtension(compressedFile);
            if (!extension.Equals(ARCHIVE_FORMAT, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidDataException("Only support *.zip");
            }

            ZipFile.ExtractToDirectory(compressedFile, outputDirectory, true);
        }
    }
}
