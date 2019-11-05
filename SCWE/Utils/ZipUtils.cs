using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SCWE.Utils
{
    public class ZipUtils
    {
        public static void Unzip(Stream s, string outFolder)
        {
            ZipArchive z = new ZipArchive(s);
            z.ExtractToDirectory(outFolder);
            z.Dispose();
        }

        public static void Unzip(string path, string outFolder)
        {
            ZipFile.ExtractToDirectory(path, outFolder);
        }
    }
}
