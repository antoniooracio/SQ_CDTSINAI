using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Models;

namespace SQ.CDT_SINAI.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Collaborator> Collaborators { get; set; }
        public DbSet<Specialization> Specializations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // O EF Core infere automaticamente a tabela de junção (Many-to-Many)
        }
    }
}