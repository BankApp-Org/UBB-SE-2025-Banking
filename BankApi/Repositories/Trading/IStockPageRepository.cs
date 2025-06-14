﻿using Common.Models.Trading;

namespace BankApi.Repositories.Trading
{
    public interface IStockPageRepository
    {
        Task AddOrUpdateUserStockAsync(string userCNP, string stockName, int quantity);
        Task AddStockValueAsync(string stockName, decimal price);
        Task<bool> GetFavoriteAsync(string userCNP, string stockName);
        Task<int> GetOwnedStocksAsync(string userCNP, string stockName);
        Task<Stock> GetStockAsync(string stockName);
        Task<UserStock> GetUserStockAsync(string userCNP, string stockName);
        Task<List<decimal>> GetStockHistoryAsync(string stockName);
        Task ToggleFavoriteAsync(string userCNP, string stockName, bool state);
    }
}