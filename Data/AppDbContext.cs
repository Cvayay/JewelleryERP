using JewelleryERP.Models;
using JewelleryERP.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<LoanPayment> LoanPayments => Set<LoanPayment>();

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
        // --- User Configuration ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            
            entity.Property(user => user.Username)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.Property(user => user.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(user => user.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
        });

        // --- Customer Configuration ---
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).IsRequired().HasMaxLength(200);
            entity.Property(customer => customer.PhoneNumber).HasMaxLength(20);
            entity.Property(customer => customer.Address).HasMaxLength(500);
            entity.Property(customer => customer.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        });

        // --- Product Configuration ---
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).IsRequired().HasMaxLength(200);
            entity.Property(product => product.Category).HasMaxLength(100);
            entity.Property(product => product.HSNCode).HasMaxLength(20); // Added HSN
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.StockQuantity).HasDefaultValue(0);
            entity.Property(product => product.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        });

        // --- Invoice Configuration ---
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(invoice => invoice.Id);
            entity.Property(invoice => invoice.InvoiceDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(invoice => invoice.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(invoice => invoice.Customer)
                .WithMany()
                .HasForeignKey(invoice => invoice.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- InvoiceItem Configuration ---
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.HSNCode).HasMaxLength(20); // Added HSN
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.LineTotal).HasPrecision(18, 2);
            
            entity.HasOne(item => item.Invoice)
                .WithMany(invoice => invoice.Items)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Setting Configuration ---
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Settings");
            entity.HasKey(setting => setting.Id);
            entity.Property(setting => setting.ShopName).IsRequired().HasMaxLength(200);
            entity.Property(setting => setting.Address).HasMaxLength(500);
            entity.Property(setting => setting.GSTIN).HasMaxLength(20);
            entity.Property(setting => setting.CurrentGoldRate).HasPrecision(18, 2);
            entity.Property(setting => setting.CurrentSilverRate).HasPrecision(18, 2);
            entity.Property(setting => setting.CGST).HasPrecision(5, 2);
            entity.Property(setting => setting.SGST).HasPrecision(5, 2);
            entity.Property(setting => setting.CurrentBillNumber).HasDefaultValue(1);
            entity.Property(setting => setting.DefaultInterestRate).HasPrecision(5, 2);
            entity.Property(setting => setting.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        });

        // --- Loan Configuration ---
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(loan => loan.Id);
            entity.Property(loan => loan.LoanNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(loan => loan.LoanNumber).IsUnique();
            entity.Property(loan => loan.LoanDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(loan => loan.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(loan => loan.Weight).HasPrecision(18, 3);
            entity.Property(loan => loan.PresentValue).HasPrecision(18, 2);
            entity.Property(loan => loan.ArticleDescription).HasMaxLength(500);
            entity.Property(loan => loan.OwnerName).HasMaxLength(200);
            entity.Property(loan => loan.OwnerAddress).HasMaxLength(500);
            entity.Property(loan => loan.RedeemerName).HasMaxLength(200);
            entity.Property(loan => loan.RedeemerAddress).HasMaxLength(500);
            entity.Property(loan => loan.Status).HasConversion<int>().HasDefaultValue(LoanStatus.Pending);
            entity.Property(loan => loan.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            
            entity.HasOne(loan => loan.Customer)
                .WithMany()
                .HasForeignKey(loan => loan.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- LoanPayment Configuration ---
        modelBuilder.Entity<LoanPayment>(entity =>
        {
            entity.ToTable("LoanPayments");
            entity.HasKey(payment => payment.Id);
            
            entity.Property(payment => payment.PaymentDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(payment => payment.AmountPaid).HasPrecision(18, 2);
            entity.Property(payment => payment.PrincipalCovered).HasPrecision(18, 2);
            entity.Property(payment => payment.InterestCovered).HasPrecision(18, 2);
            entity.Property(payment => payment.ReceiptNumber).HasMaxLength(50);
            entity.Property(payment => payment.Notes).HasMaxLength(500);

            entity.HasOne(payment => payment.Loan)
                .WithMany(loan => loan.Payments)
                .HasForeignKey(payment => payment.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}