namespace Common.Exceptions
{
    using System;

    public class GuestUserOperationException(string message) : Exception(message)
    {
    }

    public class GemTransactionFailedException(string message) : Exception(message)
    {
    }

    public class InsufficientGemsException(string message) : Exception(message)
    {
    }
}
