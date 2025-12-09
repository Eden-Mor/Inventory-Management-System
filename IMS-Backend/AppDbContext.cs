using IMS_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IMS_Backend;

// Add-Migration "NAME HERE"
// Update-Database
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<ItemPurchase> ItemPurchases { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<SupplierOrder> SupplierOrders { get; set; }
    public DbSet<SupplierOrderItem> SupplierOrderItems { get; set; }
    public DbSet<Seller> Sellers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //// Relationships
        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Supplier)
            .WithMany(s => s.Stocks)
            .HasForeignKey(s => s.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemPurchase>()
            .HasOne(pi => pi.Purchase)
            .WithMany(p => p.Items)
            .HasForeignKey(pi => pi.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemPurchase>()
            .HasOne(pi => pi.Stock)
            .WithMany(s => s.PurchaseItems)
            .HasForeignKey(pi => pi.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SupplierOrder>()
            .HasOne(o => o.Supplier)
            .WithMany(s => s.SupplierOrders)
            .HasForeignKey(o => o.SupplierId);

        modelBuilder.Entity<SupplierOrderItem>()
            .HasOne(i => i.SupplierOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.SupplierOrderId);

        modelBuilder.Entity<Seller>()
            .HasMany(s => s.Sales)
            .WithOne(p => p.Seller)
            .HasForeignKey(p => p.SellerId);

        modelBuilder.Entity<Purchase>()
            .Property(p => p.PurchaseDate)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
    }
}