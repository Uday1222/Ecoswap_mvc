using Microsoft.EntityFrameworkCore;
using EcoSwap.Models;
using Ecoswap_mvc.Models;

namespace Ecoswap_mvc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Item> Items { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<User> Users { get; set; }
    }
} 