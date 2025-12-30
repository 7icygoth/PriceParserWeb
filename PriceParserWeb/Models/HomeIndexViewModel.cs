namespace PriceParserWeb.Models
{
    public class HomeIndexViewModel
    {
        public List<ProductItem> Products { get; set; } = new();
    }

    public class ProductItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public string? BestShop { get; set; }
        public bool HasNeptun { get; set; }
        public bool HasDomotex { get; set; }
    }
}