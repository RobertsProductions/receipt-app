using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApi.Models;

namespace MyApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptShare> ReceiptShares { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Customize Identity table names if needed
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
        });

        // Configure Receipt entity
        builder.Entity<Receipt>(entity =>
        {
            entity.ToTable("Receipts");
            
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.UploadedAt);
        });

        // Configure ReceiptShare entity
        builder.Entity<ReceiptShare>(entity =>
        {
            entity.ToTable("ReceiptShares");
            
            entity.HasOne(rs => rs.Receipt)
                .WithMany()
                .HasForeignKey(rs => rs.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rs => rs.Owner)
                .WithMany()
                .HasForeignKey(rs => rs.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(rs => rs.SharedWithUser)
                .WithMany()
                .HasForeignKey(rs => rs.SharedWithUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Prevent duplicate shares: same receipt cannot be shared with the same user twice
            entity.HasIndex(rs => new { rs.ReceiptId, rs.SharedWithUserId })
                .IsUnique();

            entity.HasIndex(rs => rs.OwnerId);
            entity.HasIndex(rs => rs.SharedWithUserId);
            entity.HasIndex(rs => rs.SharedAt);
        });

        // Configure ChatMessage entity
        builder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
            
            entity.HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cm => cm.UserId);
            entity.HasIndex(cm => cm.CreatedAt);
        });
    }
}
