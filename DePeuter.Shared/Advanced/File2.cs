using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Microsoft.Win32;

namespace System.IO
{
    public static class File2
    {
        private static readonly ILog Log = LogManager.GetLogger("Global");

        public static void Delete(string fileName)
        {
            if(!File.Exists(fileName))
                return;

            while(true)
            {
                try
                {
                    File.Delete(fileName);
                    if(File.Exists(fileName))
                        return;
                    Thread.Sleep(50);
                }
                catch(UnauthorizedAccessException ex)
                {
                    Log.Error(ex);
                    //System.Threading.Thread.Sleep(50);
                    return;
                }
                catch(IOException ex)
                {
                    Log.Error(ex);
                    Thread.Sleep(50);
                }
            }
        }

        public static void CopyFile(string sourcePath, string destPath)
        {
            var info = new FileInfo(destPath);
            Directory2.CreateDirectory(info.Directory);
            File.Copy(sourcePath, destPath);
        }

        public static byte[] ReadAllBytes(string path)
        {
            byte[] bytes;
            using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int index = 0;
                long fileLength = fs.Length;
                if(fileLength > Int32.MaxValue)
                    throw new IOException("File too long");
                int count = (int)fileLength;
                bytes = new byte[count];
                while(count > 0)
                {
                    int n = fs.Read(bytes, index, count);
                    if(n == 0)
                        throw new InvalidOperationException("End of file reached before expected");
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }

        public static byte[] ReadAllBytesWithoutLock(string path)
        {
            byte[] png = null;

            while(png == null || png.Length == 0)
            {
                try
                {
                    png = File.ReadAllBytes(path);
                }
                catch
                {
                    Thread.Sleep(10);
                }
            }

            return png;
        }

        public static string GetUNCPath(string path)
        {
            if(path == null) return null;
            if(path.StartsWith(@"\\")) return path;

            var driveLetter = Directory.GetDirectoryRoot(path).Substring(0, 1);

            var currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 :  RegistryView.Registry32);
            using(var registryKey = currentUser.OpenSubKey("Network\\" + driveLetter, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
            {
                if(registryKey != null)
                {
                    var o = registryKey.GetValue("RemotePath");
                    if(o != null)
                    {
                        return o.ToString().TrimEnd('\\') + path.Substring(2);
                    }
                }
            }

            return path;

            //var driveLetter = Directory.GetDirectoryRoot(path).TrimEnd(Path.DirectorySeparatorChar);
            //using (var mo = new ManagementObject())
            //{
            //    mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveLetter));

            //    var driveType = (DriveType)(uint)(mo["DriveType"]);

            //    if (driveType == DriveType.Network)
            //    {
            //        var providerName = Convert.ToString(mo["ProviderName"]);

            //    }
            //    return path;    
            //}
        }

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            if(!File.Exists(path))
            {
                File.WriteAllBytes(path, bytes);
            }
            else
            {
                using(var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}