using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.IO
{
    public static class Directory2
    {
        public static void CopyDirectory(string sourcePath, string destPath, bool overwrite)
        {
            Directory.CreateDirectory(destPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, overwrite);
            }

            foreach (var folder in Directory.GetDirectories(sourcePath))
            {
                var dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest, overwrite);
            }
        }

        public static void CreateDirectory(DirectoryInfo directory)
        {
            if (directory.Parent != null && !directory.Parent.Exists)
            {
                CreateDirectory(directory.Parent);
            }
            directory.Create();
        }

        public static void MoveDirectory(string sourcePath, string destPath, bool overwrite = true)
        {
            Directory.CreateDirectory(destPath);

            foreach(var file in Directory.GetFiles(sourcePath))
            {
                var dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, overwrite);
                File.Delete(file);
            }

            foreach(var folder in Directory.GetDirectories(sourcePath))
            {
                var dest = Path.Combine(destPath, Path.GetFileName(folder));
                MoveDirectory(folder, dest, overwrite);
            }

            DeleteDirectory(sourcePath);
        }

        public static void DeleteDirectory(string path)
        {
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);

            foreach(var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach(var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(path, true);
        }
    }
}