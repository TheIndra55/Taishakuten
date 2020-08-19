using System;
using System.Collections.Generic;
using System.Text;

namespace Kurisu.VirusScan
{
    class NoScanResultException : Exception
    {
        public NoScanResultException()
        {
        }

        public NoScanResultException(string message)
            : base(message)
        {
        }

        public NoScanResultException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
