using System.ComponentModel.DataAnnotations;

namespace PriceParserWeb.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? UrlNeptun { get; set; }
        public string? UrlDomotex { get; set; }
    }
}