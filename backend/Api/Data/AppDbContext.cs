using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    // Uygulamanın veritabanı bağlamı.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Mesaj kayıtlarını temsil eden tablo.
        public DbSet<Message> Messages { get; set; }
    }
}
