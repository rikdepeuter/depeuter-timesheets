using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class FileInfoExtensions
{
    public static string ToSHA1(this FileInfo fi)
    {
        return Hash.SHA1FromFile(fi.FullName);
    }

    public static string NameWithoutExtension(this FileInfo fi)
    {
        return fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
    }
}