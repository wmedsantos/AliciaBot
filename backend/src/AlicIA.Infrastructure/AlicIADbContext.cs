using AlicIA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlicIA.Infrastructure.Persistence;

public class AlicIADbContext : DbContext
{
    public AlicIADbContext(DbContextOptions<AlicIADbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<CalendarConnection> CalendarConnections => Set<CalendarConnection>();
    public DbSet<BusinessHours> BusinessHours => Set<BusinessHours>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Slug).HasMaxLength(120);
            entity.Property(x => x.Segment).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Plan).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);

            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).IsRequired().HasMaxLength(50);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Price).HasPrecision(10, 2);

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Phone).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("requests");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.TotalAmount).HasPrecision(10, 2);
            entity.Property(x => x.ExternalEventId).HasMaxLength(200);
            entity.Property(x => x.ConfirmationTokenHash).HasMaxLength(64);
            entity.HasIndex(x => x.ConfirmationTokenHash).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Service)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BusinessHours>(entity =>
        {
            entity.ToTable("business_hours");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DayOfWeek).IsRequired();
            entity.Property(x => x.StartTime).IsRequired();
            entity.Property(x => x.EndTime).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.BusinessHours)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CalendarConnection>(entity =>
        {
            entity.ToTable("calendar_connections");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            entity.Property(x => x.CalendarEmail).IsRequired().HasMaxLength(200);
            entity.Property(x => x.CalendarId).IsRequired().HasMaxLength(200);
            entity.Property(x => x.RefreshToken).IsRequired();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.CalendarConnections)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
