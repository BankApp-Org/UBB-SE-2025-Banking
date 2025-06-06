using Common.Models.Bank;

namespace Common.DTOs
{
    public class TransactionTypeDTO
    {
        required public int Id { get; set; }
        required public string Description { get; set; }
        required public TransactionType TransactionType { get; set; }
    }
}
