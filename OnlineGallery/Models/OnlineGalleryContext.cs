using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OnlineGallery.Models;

public partial class OnlineGalleryContext : DbContext
{
    public OnlineGalleryContext()
    {
    }

    public OnlineGalleryContext(DbContextOptions<OnlineGalleryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Transactions> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=OnlineGallery;Trusted_Connection=True;TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Image>()
            .Property(i => i.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Transactions>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6BC18781B4");

            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    public DbSet<User> Users { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transactions> Transaction { get; set; }

}
