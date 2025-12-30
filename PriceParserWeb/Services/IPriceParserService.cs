namespace PriceParserWeb.Services
{
    public interface IPriceParserService
    {
        Task<decimal?> ParseNeptunAsync(string url);
        Task<decimal?> ParseDomotexAsync(string url);
        Task ParseAllProductsAsync();
    }
}