using Microsoft.EntityFrameworkCore;
using PriceParserWeb.Models;

namespace PriceParserWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<PriceRecord> PriceRecords => Set<PriceRecord>();
    }
}