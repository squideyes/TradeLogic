using System;

namespace WickScalper.Common
{
    /// <summary>
    /// Exception thrown when validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        /// <param name="message">The error message</param>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

