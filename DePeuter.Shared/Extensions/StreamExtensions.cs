using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream input)
        {
            var buffer = new byte[16*1024];
            using(var ms = new MemoryStream())
            {
                int read;
                while((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string ReadString(this Stream input)
        {
            var reader = new StreamReader(input);
            return reader.ReadToEnd();
        }
        public static string ReadString(this Stream input, Encoding encoding)
        {
            var reader = new StreamReader(input, encoding);
            return reader.ReadToEnd();
        }
    }
}
