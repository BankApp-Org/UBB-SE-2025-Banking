namespace Common.Exceptions
{
    using System;

    public class DuplicateStockException : Exception
    {
        public string StockName { get; }

        public DuplicateStockException(string stockName)
            : base($"Stock with name '{stockName}' already exists.")
        {
            StockName = stockName;
        }

        public DuplicateStockException(string stockName, Exception innerException)
            : base($"Stock with name '{stockName}' already exists.", innerException)
        {
            StockName = stockName;
        }
    }
}