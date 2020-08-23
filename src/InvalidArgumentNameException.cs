using System;


namespace ArdaOzcan.SimpleArgParse
{
    public class InvalidArgumentNameException : Exception
    {
        public InvalidArgumentNameException(string message) : base(message)
        {
        }
    }
}