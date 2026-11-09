namespace Berwah.Models.DTO.SupervisorUsed
{
    public class PartyDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
}