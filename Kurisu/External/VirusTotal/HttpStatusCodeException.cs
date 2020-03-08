using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Kurisu.External.VirusTotal
{
    class HttpStatusCodeException : Exception
    {
        public HttpResponseMessage HttpResponseMessage { get; private set; }

        public HttpStatusCodeException()
        { }

        public HttpStatusCodeException(HttpResponseMessage httpResponseMessage)
        {
            HttpResponseMessage = httpResponseMessage;
        }
    }
}
