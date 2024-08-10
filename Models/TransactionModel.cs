namespace BankingApp.Models
{
    public class TransactionModel
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }

    public class TransferModel
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
    }
}
