using JewelleryERP.Helpers;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Data;
public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            AppPaths.EnsureCreated();
            optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.PhoneNumber).HasMaxLength(20);
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.GSTIN).HasMaxLength(20);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.ProductCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.ProductCode).IsUnique();
            entity.Property(p => p.Barcode).HasMaxLength(100);
            entity.HasIndex(p => p.Barcode);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.Property(p => p.MetalType).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Purity).IsRequired().HasMaxLength(20);
            
            entity.Property(p => p.Weight).HasPrecision(18, 3);
            entity.Property(p => p.GrossWeight).HasPrecision(18, 3);
            entity.Property(p => p.StoneWeight).HasPrecision(18, 3);
            entity.Property(p => p.NetWeight).HasPrecision(18, 3);

            entity.Property(p => p.MetalRate).HasPrecision(18, 2);
            entity.Property(p => p.MakingCharge).HasPrecision(18, 2);
            entity.Property(p => p.MakingChargeType).IsRequired().HasMaxLength(20);
            entity.Property(p => p.StoneCost).HasPrecision(18, 2);
            entity.Property(p => p.SellingPrice).HasPrecision(18, 2);

            entity.Property(p => p.Quantity).HasDefaultValue(1);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.BillNumber).IsRequired().HasMaxLength(50);
            entity.Property(i => i.InvoiceDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(i => i.MakingChargeTotal).HasPrecision(18,2);
            entity.Property(i => i.CgstRate).HasPrecision(5,2);
            entity.Property(i => i.SgstRate).HasPrecision(5,2);
            entity.Property(i => i.CgstAmount).HasPrecision(18,2);
            entity.Property(i => i.SgstAmount).HasPrecision(18,2);
            entity.Property(i => i.TotalAmount).HasPrecision(18,2);
            entity.HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(ii => ii.Id);
            entity.Property(ii => ii.GrossWeightGms).HasPrecision(18,3);
            entity.Property(ii => ii.GrossWeightMg).HasPrecision(18,3);
            entity.Property(ii => ii.NetWeightGms).HasPrecision(18,3);
            entity.Property(ii => ii.NetWeightMg).HasPrecision(18,3);

            entity.Property(ii => ii.RatePerGram).HasPrecision(18,2);
            entity.Property(ii => ii.MakingCharge).HasPrecision(18,2);
            entity.Property(ii => ii.LineTotal).HasPrecision(18,2);

            entity.HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Settings");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.ShopName).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.GSTIN).HasMaxLength(20);
            entity.Property(s => s.CurrentGoldRate).HasPrecision(18, 2);
            entity.Property(s => s.CurrentSilverRate).HasPrecision(18, 2);
            entity.Property(s => s.CGST).HasPrecision(5, 2);
            entity.Property(s => s.SGST).HasPrecision(5, 2);
            entity.Property(s => s.CurrentBillNumber).HasDefaultValue(1);
            entity.Property(s => s.DefaultInterestRate).HasPrecision(5, 2);
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.LoanNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(l => l.LoanNumber).IsUnique();
            entity.Property(l => l.LoanDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(l => l.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(l => l.Weight).HasPrecision(18, 3);
            entity.Property(l => l.PresentValue).HasPrecision(18, 2);
            entity.Property(l => l.ArticleDescription).HasMaxLength(500);
            entity.Property(l => l.OwnerName).HasMaxLength(200);
            entity.Property(l => l.OwnerAddress).HasMaxLength(500);
            entity.Property(l => l.RedeemerName).HasMaxLength(200);
            entity.Property(l => l.RedeemerAddress).HasMaxLength(500);
            entity.Property(l => l.Status).HasConversion<int>().HasDefaultValue(LoanStatus.Pending);
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

            entity.HasOne(l => l.Customer)
                .WithMany()
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.UserName).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.Role).HasConversion<int>();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(al => al.Id);
            entity.Property(al => al.UserName).IsRequired().HasMaxLength(100);
            entity.Property(al => al.ActionType).IsRequired().HasMaxLength(100);
            entity.Property(al => al.Description).HasMaxLength(500);
            entity.Property(al => al.OldValue).HasColumnType("TEXT");
            entity.Property(al => al.NewValue).HasColumnType("TEXT");
            entity.Property(al => al.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            entity.Property(al => al.IPAddress).HasMaxLength(50);
            
            entity.HasIndex(al => al.CreatedAt).IsDescending();
            entity.HasIndex(al => al.UserName);
        });
    }
}
    


