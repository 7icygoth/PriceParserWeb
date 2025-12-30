using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceParserWeb.Data;
using PriceParserWeb.Models;
using PriceParserWeb.Services;

namespace PriceParserWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPriceParserService _parser;

        public HomeController(AppDbContext db, IPriceParserService parser)
        {
            _db = db;
            _parser = parser;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.ToListAsync();

            var productItems = products.Select(p =>
            {
                var priceRecords = _db.PriceRecords
                    .Where(pr => pr.ProductId == p.Id)
                    .AsEnumerable()
                    .ToList();

                return new ProductItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    MinPrice = priceRecords.Any() ? priceRecords.Min(pr => pr.Price) : null,
                    BestShop = priceRecords.Any() ? priceRecords.OrderBy(pr => pr.Price).First().Shop : null,
                    HasNeptun = !string.IsNullOrEmpty(p.UrlNeptun),
                    HasDomotex = !string.IsNullOrEmpty(p.UrlDomotex)
                };
            }).ToList();

            var model = new HomeIndexViewModel { Products = productItems };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product model)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Add(model);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RunParser()
        {
            await _parser.ParseAllProductsAsync();
            TempData["Message"] = "Парсинг завершён.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.PriceRecords.RemoveRange(_db.PriceRecords.Where(pr => pr.ProductId == id));
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}