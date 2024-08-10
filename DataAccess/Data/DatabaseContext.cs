using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class DatabaseContext:IdentityDbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        //=======================================================Authentication
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }

        //=======================================================Album
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumCategory> AlbumCategories { get; set; }
        public DbSet<AlbumProducer> AlbumProducers { get; set; }

        //=======================================================Game
        public DbSet<Game> Games { get; set; }
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<GameProducer> GameProducers { get; set; }

        //=======================================================Movie
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieCategory> MovieCategories { get; set; }
        public DbSet<MovieProducer> MovieProducers { get; set; }

        //=======================================================Sub Table Of Product
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ContentType> ContentTypes { get; set; }

        //=======================================================News
        public DbSet<News> News { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        //=======================================================Main Banner
        public DbSet<MainBanner> MainBanners { get; set; }

        //=======================================================Recommend Product Slider
        public DbSet<RecommendProductSlider> RecommendProductSliders { get; set; }

        //=======================================================Contact
        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Review> Reviewes { get; set; }


        //=======================================================No Duplicate Name When Created
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //=======================================================Album
            modelBuilder.Entity<Artist>()
                .HasIndex(a => a.ArtistName)
                .IsUnique();

            modelBuilder.Entity<Album>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<AlbumCategory>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<AlbumProducer>()
                .HasIndex(a => a.ProducerName)
                .IsUnique();

            //=======================================================Game
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<GameCategory>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<GameProducer>()
                .HasIndex(g => g.ProducerName)
                .IsUnique();

            //=======================================================Movie
            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<MovieCategory>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<MovieProducer>()
                .HasIndex(m => m.ProducerName)
                .IsUnique();

            //=======================================================Sub Table Of Product
            modelBuilder.Entity<Promotion>()
                .HasIndex(p => p.Description)
                .IsUnique();

            modelBuilder.Entity<ContentType>()
                .HasIndex(c => c.Type)
                .IsUnique();

            //=======================================================News
            modelBuilder.Entity<News>()
                .HasIndex(n => n.Title)
                .IsUnique();

            //=======================================================Main Banner
            modelBuilder.Entity<MainBanner>()
                .HasIndex(m => m.OrderDisplay)
                .IsUnique();

        }

    }
}
