using GameKeysStore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameKeysStore.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameKey> GameKeys => Set<GameKey>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<SellerRating> SellerRatings => Set<SellerRating>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>(); // 👈 nuevo

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → SellerProfile (1 a 1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.SellerProfile)
            .WithOne(s => s.User)
            .HasForeignKey<SellerProfile>(s => s.UserId);

        // SellerProfile → GameKeys
        modelBuilder.Entity<GameKey>()
            .HasOne(k => k.Seller)
            .WithMany(s => s.Keys)
            .HasForeignKey(k => k.SellerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // GameKey → Game
        modelBuilder.Entity<GameKey>()
            .HasOne(k => k.Game)
            .WithMany(g => g.Keys)
            .HasForeignKey(k => k.GameId);

        // GameKey precio
        modelBuilder.Entity<GameKey>()
            .Property(k => k.Price)
            .HasColumnType("decimal(18,2)");

        // Order → Buyer
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Buyer)
            .WithMany()
            .HasForeignKey(o => o.BuyerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // OrderItem → Order
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        // OrderItem → GameKey (1 a 1)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.GameKey)
            .WithOne(k => k.OrderItem)
            .HasForeignKey<OrderItem>(oi => oi.GameKeyId);

        // OrderItem → Game
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Game)
            .WithMany()
            .HasForeignKey(oi => oi.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem → Seller
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Seller)
            .WithMany()
            .HasForeignKey(oi => oi.SellerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem precio
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        // SellerRating → SellerProfile
        modelBuilder.Entity<SellerRating>()
            .HasOne(r => r.Seller)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.SellerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // SellerRating → Buyer
        modelBuilder.Entity<SellerRating>()
            .HasOne(r => r.Buyer)
            .WithMany()
            .HasForeignKey(r => r.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SellerRating → Order
        modelBuilder.Entity<SellerRating>()
            .HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un usuario solo puede calificar una vez por orden y vendedor
        modelBuilder.Entity<SellerRating>()
            .HasIndex(r => new { r.BuyerId, r.OrderId, r.SellerProfileId })
            .IsUnique();

        // CartItem → User
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CartItem → GameKey
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.GameKey)
            .WithMany()
            .HasForeignKey(c => c.GameKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un usuario no puede tener la misma key dos veces
        modelBuilder.Entity<CartItem>()
            .HasIndex(c => new { c.UserId, c.GameKeyId })
            .IsUnique();

        // WithdrawalRequest → SellerProfile
        modelBuilder.Entity<WithdrawalRequest>()
            .HasOne(w => w.SellerProfile)
            .WithMany(s => s.WithdrawalRequests)
            .HasForeignKey(w => w.SellerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // WithdrawalRequest monto
        modelBuilder.Entity<WithdrawalRequest>()
            .Property(w => w.Amount)
            .HasColumnType("decimal(18,2)");
    }
}