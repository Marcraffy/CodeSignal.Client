using System;

namespace CodeSignal.Exceptions
{
    public class GraphException : Exception
    {
        public GraphException() : base()
        {
        }

        public GraphException(string message) : base(message)
        {
        }

        public GraphException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
