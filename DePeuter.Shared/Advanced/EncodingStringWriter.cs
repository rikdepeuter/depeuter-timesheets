using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Advanced
{
    public class EncodingStringWriter : StringWriter
    {
        public override Encoding Encoding { get { return _encoding; } }

        private readonly Encoding _encoding;
        public EncodingStringWriter(Encoding encoding)
        {
            _encoding = encoding;
        }
    }
}
