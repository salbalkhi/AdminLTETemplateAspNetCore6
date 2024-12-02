using Microsoft.EntityFrameworkCore;
using Tadawi.Models;

namespace Tadawi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Email)
                  .HasMaxLength(100)
                  .IsRequired();
            
            entity.Property(e => e.Username)
                  .HasMaxLength(50)
                  .IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            
            entity.HasOne(e => e.User)
                  .WithMany(e => e.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.Property(e => e.Title)
                  .HasMaxLength(200)
                  .IsRequired();
            
            entity.Property(e => e.Description)
                  .HasMaxLength(500)
                  .IsRequired();
            
            entity.Property(e => e.Type)
                  .HasMaxLength(50)
                  .IsRequired();
            
            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Tags)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  );

            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.ContentType);
            entity.HasIndex(e => e.UploadedAt);
            
            entity.Property(e => e.FileName)
                  .HasMaxLength(255)
                  .IsRequired();
            
            entity.Property(e => e.ContentType)
                  .HasMaxLength(100)
                  .IsRequired();
            
            entity.Property(e => e.StoragePath)
                  .HasMaxLength(1000)
                  .IsRequired();
            
            entity.Property(e => e.PublicUrl)
                  .HasMaxLength(1000)
                  .IsRequired();

            entity.HasOne(e => e.Uploader)
                  .WithMany()
                  .HasForeignKey(e => e.UploaderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Content)
                  .WithMany()
                  .HasForeignKey(e => e.ContentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
