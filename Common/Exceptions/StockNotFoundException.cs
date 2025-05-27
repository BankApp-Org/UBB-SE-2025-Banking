namespace Common.Exceptions
{
    using System;

    public class StockNotFoundException : Exception
    {
        public string StockName { get; }

        public StockNotFoundException(string stockName)
            : base($"Stock with name '{stockName}' was not found.")
        {
            StockName = stockName;
        }

        public StockNotFoundException(string stockName, Exception innerException)
            : base($"Stock with name '{stockName}' was not found.", innerException)
        {
            StockName = stockName;
        }
    }
}