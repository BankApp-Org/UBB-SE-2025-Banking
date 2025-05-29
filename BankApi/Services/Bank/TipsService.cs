namespace BankApi.Services.Bank
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApi.Repositories;
    using BankApi.Repositories.Bank;
    using Common.Models;
    using Common.Services.Social;

    public class TipsService(ITipsRepository tipsRepository, IUserRepository userRepository) : ITipsService
    {
        private readonly ITipsRepository tipsRepository = tipsRepository;
        private readonly IUserRepository userRepository = userRepository;

        public async Task GiveTipToUserAsync(string userCNP)
        {

            try
            {
                User user = await userRepository.GetByCnpAsync(userCNP) ?? throw new Exception("User not found");
                if (user.CreditScore < 300)
                {
                    await tipsRepository.GiveLowBracketTipAsync(userCNP);
                }
                else if (user.CreditScore < 550)
                {
                    await tipsRepository.GiveMediumBracketTipAsync(userCNP);
                }
                else if (user.CreditScore > 549)
                {
                    await tipsRepository.GiveHighBracketTipAsync(userCNP);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{exception.Message},User is not found");
            }
        }
        public async Task<List<Tip>> GetTipsForUserAsync(string userCnp)
        {
            return await tipsRepository.GetTipsForUserAsync(userCnp);
        }
    }
}
