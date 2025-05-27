namespace Common.Exceptions
{
    using System;

    public class RepositoryPersistenceException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }

    public class UserRepositoryException(string message, Exception? innerException = null) : RepositoryPersistenceException(message, innerException)
    {
    }

    public class StockRepositoryException(string message, Exception? innerException = null) : RepositoryPersistenceException(message, innerException)
    {
    }

    public class TransactionRepositoryException(string message, Exception? innerException = null) : RepositoryPersistenceException(message, innerException)
    {
    }

    public class NewsRepositoryException(string message, Exception? innerException = null) : RepositoryPersistenceException(message, innerException)
    {
    }

    public class AlertRepositoryException(string message, Exception? innerException = null) : RepositoryPersistenceException(message, innerException)
    {
    }

    public class ProfilePersistenceException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }
}
