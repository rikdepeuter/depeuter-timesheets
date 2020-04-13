using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public static class ByteExtensions
{
    public static MemoryStream ToMemoryStream(this byte[] bytes)
    {
        if (bytes == null)
        {
            throw new Exception("Can't create MemoryStream from null");
        }
        var ms = new MemoryStream(bytes.Length);
        foreach (var b in bytes)
            ms.WriteByte(b);
        return ms;
    }
}