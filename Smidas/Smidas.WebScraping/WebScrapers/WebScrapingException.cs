using System;
using System.Runtime.Serialization;

namespace Smidas.WebScraping.WebScrapers
{
    [Serializable]
    public class WebScrapingException : Exception
    {
        public WebScrapingException() : base() { }
        public WebScrapingException(string message) : base(message) { }
        public WebScrapingException(string message, Exception innerException) : base(message, innerException) { }
        public WebScrapingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
