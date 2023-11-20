using DataAccess.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Comment> Comments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>()
                .Property(u => u.ImageUrl)
                .IsRequired(false);

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserId)
                .IsRequired(false);

            modelBuilder.Entity<Comment>()
                .Property(c => c.ParentCommentId)
                .IsRequired(false);

            modelBuilder.Entity<Genre>()
                .HasData(
                new Genre { Id = 1, Name = "Strategy" },
                new Genre { Id = 2, Name = "Rally", ParentGenreId = 1 },
                new Genre { Id = 3, Name = "Arcade", ParentGenreId = 1 },
                new Genre { Id = 4, Name = "Formula", ParentGenreId = 1 },
                new Genre { Id = 5, Name = "Off-road", ParentGenreId = 1 },
                new Genre { Id = 6, Name = "Rpg" },
                new Genre { Id = 7, Name = "Sports" },
                new Genre { Id = 8, Name = "Races" },
                new Genre { Id = 9, Name = "Action" },
                new Genre { Id = 10, Name = "Fps", ParentGenreId = 9 },
                new Genre { Id = 11, Name = "Tps", ParentGenreId = 9 },
                new Genre { Id = 12, Name = "Misc", ParentGenreId = 9 },
                new Genre { Id = 13, Name = "Adventure" },
                new Genre { Id = 14, Name = "Puzzle & Skill" },
                new Genre { Id = 15, Name = "Other" }
                );


            /*            modelBuilder.Entity<Game>()
                            .HasMany(g => g.Genres)
                            .WithMany(g => g.Games)
                            .UsingEntity(j => j.ToTable("GameGenres"));

                        modelBuilder.Entity<Genre>()
                            .HasOne(g => g.ParentGenre)
                            .WithMany(g => g.SubGenres)
                            .HasForeignKey(g => g.ParentId)
                            .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.Cascade, depending on your requirements
            */





        }
        /*       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
               {
                    optionsBuilder.UseSqlServer(
                        @"Server = .\SQLEXPRESS; database = ToDoDB; Integrated Security = true; trustServerCertificate = true");

               }
       */

    }
}