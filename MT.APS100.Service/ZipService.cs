using System;
using System.Diagnostics;

namespace MT.APS100.Service
{
    public class ZipService : IDisposable
    {
        // Fields  
        private string _7zInstallPath = @"C:\Program Files\7-Zip\7z.exe";

        public void CompressFile(string strInFilePath, string strOutFilePath, string type)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = this._7zInstallPath;
                process.StartInfo.Arguments = " a -t" + type + " " + strOutFilePath + " " + strInFilePath + "";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                process.Close();
            }
        }
 
        public void DecompressFileToDestDirectory(string strInFilePath, string strOutDirectoryPath = "")
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = this._7zInstallPath;
                process.StartInfo.Arguments = " x " + strInFilePath + " -o" + strOutDirectoryPath + " -r -y";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                process.Close();
            }
        }

        public void Dispose()
        {
            
        }
    }
}