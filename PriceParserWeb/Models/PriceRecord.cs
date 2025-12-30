namespace PriceParserWeb.Models
{
    public class PriceRecord
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Shop { get; set; } = string.Empty; // "neptun66", "domotex"
        public decimal Price { get; set; }
        public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
    }
}