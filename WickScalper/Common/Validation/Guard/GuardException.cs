using System;

namespace WickScalper.Common
{
    public class GuardException : Exception
    {
        public GuardException(string message) 
            : base(message)
        {
        }

        public GuardException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

