namespace BankApi.Services
{
    using BankApi.Repositories;
    using Common.Exceptions;
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class HistoryService(IStockHistoryRepository historyRepository) : IHistoryService
    {
        private readonly IStockHistoryRepository _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));

        public async Task<List<CreditScoreHistory>> GetAllHistoryAsync()
        {
            try
            {
                return await _historyRepository.GetAllHistoryAsync();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving all credit score history", ex);
            }
        }

        public async Task<CreditScoreHistory> GetHistoryByIdAsync(int id)
        {
            try
            {
                return await _historyRepository.GetHistoryByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error retrieving history entry with ID {id}", ex);
            }
        }

        public async Task AddHistoryAsync(CreditScoreHistory history)
        {
            ArgumentNullException.ThrowIfNull(history);

            if (history.Score < 0 || history.Score > 1000)
            {
                throw new HistoryServiceException("Credit score must be between 0 and 1000");
            }

            try
            {
                await _historyRepository.AddHistoryAsync(history);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error adding credit score history entry", ex);
            }
        }

        public async Task UpdateHistoryAsync(CreditScoreHistory history)
        {
            ArgumentNullException.ThrowIfNull(history);

            if (history.Score < 0 || history.Score > 1000)
            {
                throw new HistoryServiceException("Credit score must be between 0 and 1000");
            }

            try
            {
                await _historyRepository.UpdateHistoryAsync(history);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error updating history entry with ID {history.Id}", ex);
            }
        }

        public async Task DeleteHistoryAsync(int id)
        {
            try
            {
                await _historyRepository.DeleteHistoryAsync(id);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error deleting history entry with ID {id}", ex);
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _historyRepository.GetHistoryForUserAsync(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving credit score history for user", ex);
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryWeeklyAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _historyRepository.GetHistoryWeeklyAsync(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving weekly credit score history", ex);
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryMonthlyAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _historyRepository.GetHistoryMonthlyAsync(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving monthly credit score history", ex);
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryYearlyAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _historyRepository.GetHistoryYearlyAsync(userCnp);
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving yearly credit score history", ex);
            }
        }
    }
}
