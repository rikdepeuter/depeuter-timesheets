using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Exceptions
{
    public class AsyncException : Exception
    {
        public AsyncException(Exception exception)
            : base("Exception on async thread", exception)
        {
        }
    }
}
