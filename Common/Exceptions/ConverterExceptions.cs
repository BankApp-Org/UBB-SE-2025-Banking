namespace Common.Exceptions
{
    using System;

    public class ConverterException : Exception
    {
        public ConverterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ConverterException(string message)
            : base(message)
        {
        }
    }
}
