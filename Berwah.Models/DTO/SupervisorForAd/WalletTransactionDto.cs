namespace Berwah.Models.DTO.SupervisorForAd
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string DateAgo { get; set; }
    }
}