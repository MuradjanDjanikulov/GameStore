using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DataAccess.Entity;

namespace DataAccess.Repository
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>()
                .Property(u => u.ImageUrl)
                .IsRequired(false);
            //           modelBuilder.Entity<Employee>().Property(e => e.Email).HasDefaultValueSql("'test@mail.com'");

            //           modelBuilder.Entity<Address>().HasData(new Address(999, "Navoiy street", "UZB", "TASH"));

            //Add-Migration and Update-Databse
        }
        /*       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
               {
                    optionsBuilder.UseSqlServer(
                        @"Server = .\SQLEXPRESS; database = ToDoDB; Integrated Security = true; trustServerCertificate = true");

               }
       */

    }
}