namespace RealtorsPortal.Models.Entities
{
    public class SiteSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string? Value { get; set; }
        public string? Group { get; set; }    // "General" | "Listings" | "Payment"
        public DateTime UpdatedAt { get; set; }
    }
}
