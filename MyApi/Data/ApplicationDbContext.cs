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
    }
}
