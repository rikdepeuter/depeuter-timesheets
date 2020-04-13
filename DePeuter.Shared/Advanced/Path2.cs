using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class Path2
    {
        public static string ToValidPath(string filename, string invalidCharacterReplacement = null)
        {
            if(filename == null) return null;
            var illegalChars = Path.GetInvalidPathChars();

            if(invalidCharacterReplacement == null) invalidCharacterReplacement = string.Empty;

            foreach(var c in illegalChars)
            {
                filename = filename.Replace(c.ToString(), invalidCharacterReplacement);
            }

            return filename;
        }

        public static string ToValidFileName(string filename, string invalidCharacterReplacement = null)
        {
            if (filename == null) return null;
            var illegalChars = Path.GetInvalidFileNameChars();

            if(invalidCharacterReplacement == null) invalidCharacterReplacement = string.Empty;

            foreach(var c in illegalChars)
            {
                filename = filename.Replace(c.ToString(), invalidCharacterReplacement);
            }

            return filename;
        }
    }
}
