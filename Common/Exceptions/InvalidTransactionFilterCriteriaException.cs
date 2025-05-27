namespace Common.Exceptions
{
    public class InvalidTransactionFilterCriteriaException : Exception
    {
        public InvalidTransactionFilterCriteriaException(string message) : base(message)
        {
        }
        public InvalidTransactionFilterCriteriaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
