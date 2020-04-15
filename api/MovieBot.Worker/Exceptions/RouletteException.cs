using System;
using System.Runtime.Serialization;

namespace MovieBot.Worker.Exceptions
{
    public class RouletteException : Exception
    {
        public RouletteException()
        {
        }

        protected RouletteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RouletteException(string message) : base(message)
        {
        }

        public RouletteException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}