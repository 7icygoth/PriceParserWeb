using Microsoft.Playwright;
using Microsoft.EntityFrameworkCore;
using PriceParserWeb.Data;
using PriceParserWeb.Models;
using Microsoft.Extensions.Logging;

namespace PriceParserWeb.Services
{
    public class PriceParserService : IPriceParserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PriceParserService> _logger;

        public PriceParserService(AppDbContext context, ILogger<PriceParserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<decimal?> ParseNeptunAsync(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            try
            {
                await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
                await page.WaitForSelectorAsync(".product-price", new() { Timeout = 10000 });

                var priceText = await page.TextContentAsync(".product-price");
                if (string.IsNullOrWhiteSpace(priceText)) return null;

                priceText = priceText.Replace("\u00A0", " ");
                var clean = new string(priceText.Where(c => char.IsDigit(c) || c == ',' || c == '.' || c == ' ').ToArray())
                    .Replace(" ", "").Replace(',', '.');

                if (decimal.TryParse(clean, out var price))
                {
                    // === ЛОГИРОВАНИЕ ===
                    _logger.LogInformation("Успешный парсинг: магазин=neptun66, url={Url}, цена={Price:C}, время={Time}",
                        url, price, DateTime.UtcNow);

                    return price;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге neptun66: url={Url}", url);
                return null;
            }
        }

        public async Task<decimal?> ParseDomotexAsync(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            try
            {
                await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
                await page.WaitForSelectorAsync(".price_value", new() { Timeout = 10000 });

                var priceText = await page.TextContentAsync(".price_value");
                if (string.IsNullOrWhiteSpace(priceText)) return null;

                priceText = priceText.Replace("\u00A0", " ");
                var clean = new string(priceText.Where(c => char.IsDigit(c) || c == ',' || c == '.' || c == ' ').ToArray())
                    .Replace(" ", "").Replace(',', '.');

                if (decimal.TryParse(clean, out var price))
                {
                    // === ЛОГИРОВАНИЕ ===
                    _logger.LogInformation("Успешный парсинг: магазин=domotex, url={Url}, цена={Price:C}, время={Time}",
                        url, price, DateTime.UtcNow);

                    return price;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге domotex: url={Url}", url);
                return null;
            }
        }

        public async Task ParseAllProductsAsync()
        {
            var products = _context.Products.ToList();
            bool hasNew = false;

            foreach (var p in products)
            {
                if (!string.IsNullOrWhiteSpace(p.UrlNeptun))
                {
                    var price = await ParseNeptunAsync(p.UrlNeptun);
                    if (price.HasValue)
                    {
                        _context.PriceRecords.Add(new PriceRecord
                        {
                            ProductId = p.Id,
                            Shop = "neptun66",
                            Price = price.Value
                        });
                        hasNew = true;
                    }
                }

                if (!string.IsNullOrWhiteSpace(p.UrlDomotex))
                {
                    var price = await ParseDomotexAsync(p.UrlDomotex);
                    if (price.HasValue)
                    {
                        _context.PriceRecords.Add(new PriceRecord
                        {
                            ProductId = p.Id,
                            Shop = "domotex",
                            Price = price.Value
                        });
                        hasNew = true;
                    }
                }
            }

            if (hasNew)
                await _context.SaveChangesAsync();
        }
    }
}