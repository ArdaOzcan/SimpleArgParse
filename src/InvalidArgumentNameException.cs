using System;


namespace ArdaOzcan.SimpleArgParse
{
    internal class InvalidArgumentNameException : Exception
    {
        public InvalidArgumentNameException(string message) : base(message)
        {
        }
    }
}