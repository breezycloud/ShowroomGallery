using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ShowroomAPI.Models;

#nullable disable

namespace ShowroomAPI.Context
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<StockIn> StockIns { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionDetail> TransactionDetails { get; set; }
        public virtual DbSet<UserPermission> UserPermissions { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<staff> staff { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DefaultConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryNo);

                entity.ToTable("Category");

                entity.Property(e => e.CategoryNo).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.CategoryName).HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(100);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PayId);

                entity.ToTable("Payment");

                entity.HasIndex(e => e.PayId, "UQ__Payment__000000000000003A")
                    .IsUnique();

                entity.Property(e => e.PayId).HasColumnName("PayID");

                entity.Property(e => e.Cash).HasColumnType("money");

                entity.Property(e => e.InvoiceNo).HasMaxLength(4);

                entity.Property(e => e.Pchange)
                    .HasColumnType("money")
                    .HasColumnName("PChange");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductNo);

                entity.HasIndex(e => e.ModelNo, "UQ__Products__0000000000000186")
                    .IsUnique();

                entity.Property(e => e.ProductNo).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.Barcode).HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(100);

                entity.Property(e => e.ModelNo).HasMaxLength(20);

                entity.Property(e => e.ProductCode).HasMaxLength(100);

                entity.Property(e => e.UnitPrice).HasColumnType("money");

                entity.HasOne(d => d.CategoryNoNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryNo)
                    .HasConstraintName("FK_Category_Products");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.TokenId)
                    .HasName("PK__RefreshT__658FEE8A50080EEC");

                entity.ToTable("RefreshToken");

                entity.Property(e => e.TokenId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("TokenID");

                entity.Property(e => e.ExpiryDate).HasPrecision(0);

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.Property(e => e.Token).IsRequired();

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__RefreshTo__Staff__29221CFB");
            });

            modelBuilder.Entity<StockIn>(entity =>
            {
                entity.HasKey(e => e.StockInNo);

                entity.ToTable("StockIn");

                entity.HasIndex(e => e.StockInNo, "UQ__StockIn__0000000000000066")
                    .IsUnique();

                entity.Property(e => e.DateIn).HasColumnType("date");

                entity.HasOne(d => d.ProductNoNavigation)
                    .WithMany(p => p.StockIns)
                    .HasForeignKey(d => d.ProductNo)
                    .HasConstraintName("FK_StockIn_Products");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasIndex(e => e.InvoiceNo, "UQ_Transactions_InvoiceNo")
                    .IsUnique();

                entity.HasIndex(e => e.Id, "UQ__Transactions__000000000000017D")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Discount).HasColumnType("money");

                entity.Property(e => e.InvoiceNo)
                    .IsRequired()
                    .HasMaxLength(4);

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.Property(e => e.SubTotal).HasColumnType("money");

                entity.Property(e => e.Tdate)
                    .HasColumnType("date")
                    .HasColumnName("TDate");

                entity.Property(e => e.TotalAmount).HasColumnType("money");

                entity.Property(e => e.Ttime)
                    .HasColumnType("time(0)")
                    .HasColumnName("TTime");
            });

            modelBuilder.Entity<TransactionDetail>(entity =>
            {
                entity.HasKey(e => e.TdetailsNo);

                entity.HasIndex(e => e.TdetailsNo, "UQ__TransactionDetails__000000000000007C")
                    .IsUnique();

                entity.Property(e => e.TdetailsNo).HasColumnName("TDetailsNo");

                entity.Property(e => e.InvoiceNo)
                    .IsRequired()
                    .HasMaxLength(4);

                entity.Property(e => e.ItemPrice).HasColumnType("money");

                entity.HasOne(d => d.InvoiceNoNavigation)
                    .WithMany(p => p.TransactionDetails)
                    .HasPrincipalKey(p => p.InvoiceNo)
                    .HasForeignKey(d => d.InvoiceNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionDetails_Transactions");

                entity.HasOne(d => d.ProductNoNavigation)
                    .WithMany(p => p.TransactionDetails)
                    .HasForeignKey(d => d.ProductNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionDetails_Products");
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MenuName)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK__UserRole__8AFACE3AD62190B0");

                entity.ToTable("UserRole");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.RoleDesc)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValueSql("('N/A')");
            });

            modelBuilder.Entity<staff>(entity =>
            {
                entity.ToTable("Staff");

                entity.HasIndex(e => e.StaffId, "UQ__Staff__0000000000000018")
                    .IsUnique();

                entity.Property(e => e.StaffId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("StaffID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.City).HasMaxLength(100);

                entity.Property(e => e.ContactNo).HasMaxLength(100);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.Username).HasMaxLength(100);

                entity.HasOne(d => d.UserRole)
                    .WithOne(p => p.staff)
                    .HasForeignKey<staff>(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Staff_UserRole");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
