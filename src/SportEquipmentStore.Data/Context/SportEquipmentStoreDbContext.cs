using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Enums;

namespace SportEquipmentStore.Data.Context;

public class SportEquipmentStoreDbContext : DbContext
{
    public SportEquipmentStoreDbContext(DbContextOptions<SportEquipmentStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureRoles(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureCustomers(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureProducts(modelBuilder);
        ConfigureCarts(modelBuilder);
        ConfigureCartItems(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureOrderDetails(modelBuilder);
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Role>();

        entity.ToTable("Roles", table =>
            table.HasCheckConstraint("CK_Roles_RoleName_NotBlank", "LEN(LTRIM(RTRIM([RoleName]))) > 0"));

        entity.HasKey(role => role.RoleId);
        entity.Property(role => role.RoleId).UseIdentityColumn();
        entity.Property(role => role.RoleName).HasMaxLength(30).IsRequired();
        entity.HasIndex(role => role.RoleName).IsUnique().HasDatabaseName("UX_Roles_RoleName");
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("Users", table =>
        {
            table.HasCheckConstraint("CK_Users_Username_Trimmed", "DATALENGTH([Username]) = DATALENGTH(LTRIM(RTRIM([Username]))) AND LEN(LTRIM(RTRIM([Username]))) > 0");
            table.HasCheckConstraint("CK_Users_Email_Trimmed", "DATALENGTH([Email]) = DATALENGTH(LTRIM(RTRIM([Email]))) AND LEN(LTRIM(RTRIM([Email]))) > 0");
            table.HasCheckConstraint("CK_Users_PasswordHash_NotBlank", "LEN(LTRIM(RTRIM([PasswordHash]))) > 0");
            table.HasCheckConstraint("CK_Users_FullName_NotBlank", "LEN(LTRIM(RTRIM([FullName]))) > 0");
        });

        entity.HasKey(user => user.UserId);
        entity.Property(user => user.UserId).UseIdentityColumn();
        entity.Property(user => user.Username).HasMaxLength(50).IsRequired().UseCollation("Latin1_General_100_CI_AS");
        entity.Property(user => user.Email).HasMaxLength(254).IsRequired().UseCollation("Latin1_General_100_CI_AS");
        entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        entity.Property(user => user.FullName).HasMaxLength(150).IsRequired();
        entity.Property(user => user.IsActive).HasDefaultValue(true);
        entity.Property(user => user.CreatedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(user => user.Username).IsUnique().HasDatabaseName("UX_Users_Username");
        entity.HasIndex(user => user.Email).IsUnique().HasDatabaseName("UX_Users_Email");

        entity.HasOne(user => user.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureCustomers(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Customer>();

        entity.ToTable("Customers");
        entity.HasKey(customer => customer.CustomerId);
        entity.Property(customer => customer.CustomerId).UseIdentityColumn();
        entity.Property(customer => customer.Phone).HasMaxLength(20);
        entity.Property(customer => customer.Address).HasMaxLength(500);

        entity.HasIndex(customer => customer.UserId).IsUnique().HasDatabaseName("UX_Customers_UserId");

        entity.HasOne(customer => customer.User)
            .WithOne(user => user.Customer)
            .HasForeignKey<Customer>(customer => customer.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Category>();

        entity.ToTable("Categories", table =>
            table.HasCheckConstraint("CK_Categories_CategoryName_NotBlank", "LEN(LTRIM(RTRIM([CategoryName]))) > 0"));

        entity.HasKey(category => category.CategoryId);
        entity.Property(category => category.CategoryId).UseIdentityColumn();
        entity.Property(category => category.CategoryName).HasMaxLength(120).IsRequired();
        entity.Property(category => category.Description).HasMaxLength(1000);
        entity.Property(category => category.IsActive).HasDefaultValue(true);
    }

    private static void ConfigureProducts(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Product>();

        entity.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_ProductName_NotBlank", "LEN(LTRIM(RTRIM([ProductName]))) > 0");
            table.HasCheckConstraint("CK_Products_Price_NonNegative", "[Price] >= 0");
            table.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
        });

        entity.HasKey(product => product.ProductId);
        entity.Property(product => product.ProductId).UseIdentityColumn();
        entity.Property(product => product.ProductName).HasMaxLength(200).IsRequired();
        entity.Property(product => product.Price).HasPrecision(18, 2);
        entity.Property(product => product.StockQuantity).HasDefaultValue(0);
        entity.Property(product => product.Description).HasMaxLength(2000);
        entity.Property(product => product.ImageUrl).HasMaxLength(2048);
        entity.Property(product => product.IsActive).HasDefaultValue(true);
        entity.Property(product => product.CreatedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");
        entity.Property(product => product.RowVersion).IsRowVersion();

        entity.HasIndex(product => new { product.CategoryId, product.IsActive })
            .HasDatabaseName("IX_Products_CategoryId_IsActive");
        entity.HasIndex(product => product.ProductName)
            .HasDatabaseName("IX_Products_ProductName");

        entity.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureCarts(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Cart>();

        entity.ToTable("Carts");
        entity.HasKey(cart => cart.CartId);
        entity.Property(cart => cart.CartId).UseIdentityColumn();
        entity.Property(cart => cart.CreatedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(cart => cart.CustomerId).IsUnique().HasDatabaseName("UX_Carts_CustomerId");

        entity.HasOne(cart => cart.Customer)
            .WithOne(customer => customer.Cart)
            .HasForeignKey<Cart>(cart => cart.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureCartItems(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CartItem>();

        entity.ToTable("CartItems", table =>
            table.HasCheckConstraint("CK_CartItems_Quantity_Positive", "[Quantity] > 0"));

        entity.HasKey(item => item.CartItemId);
        entity.Property(item => item.CartItemId).UseIdentityColumn();
        entity.Property(item => item.AddedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(item => new { item.CartId, item.ProductId })
            .IsUnique()
            .HasDatabaseName("UX_CartItems_CartId_ProductId");

        entity.HasOne(item => item.Cart)
            .WithMany(cart => cart.CartItems)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(item => item.Product)
            .WithMany(product => product.CartItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();

        entity.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_TotalAmount_NonNegative", "[TotalAmount] >= 0");
            table.HasCheckConstraint("CK_Orders_Status_Valid", "[Status] IN (N'Pending', N'Confirmed', N'Shipping', N'Completed', N'Cancelled')");
            table.HasCheckConstraint("CK_Orders_ShippingFullName_NotBlank", "LEN(LTRIM(RTRIM([ShippingFullName]))) > 0");
            table.HasCheckConstraint("CK_Orders_ShippingPhone_NotBlank", "LEN(LTRIM(RTRIM([ShippingPhone]))) > 0");
            table.HasCheckConstraint("CK_Orders_ShippingAddress_NotBlank", "LEN(LTRIM(RTRIM([ShippingAddress]))) > 0");
        });

        entity.HasKey(order => order.OrderId);
        entity.Property(order => order.OrderId).UseIdentityColumn();
        entity.Property(order => order.OrderDate).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()");
        entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
        entity.Property(order => order.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar(20)")
            .UseCollation("Latin1_General_100_BIN2")
            .HasDefaultValue(OrderStatus.Pending);
        entity.Property(order => order.ShippingFullName).HasMaxLength(150).IsRequired();
        entity.Property(order => order.ShippingPhone).HasMaxLength(20).IsRequired();
        entity.Property(order => order.ShippingAddress).HasMaxLength(500).IsRequired();
        entity.Property(order => order.Note).HasMaxLength(1000);
        entity.Property(order => order.RowVersion).IsRowVersion();

        entity.HasIndex(order => new { order.CustomerId, order.CheckoutRequestId })
            .IsUnique()
            .HasDatabaseName("UX_Orders_CustomerId_CheckoutRequestId");
        entity.HasIndex(order => new { order.CustomerId, order.OrderDate })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Orders_CustomerId_OrderDate");
        entity.HasIndex(order => new { order.Status, order.OrderDate })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Orders_Status_OrderDate");
        entity.HasIndex(order => order.OrderDate)
            .IsDescending(true)
            .HasDatabaseName("IX_Orders_OrderDate");

        entity.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureOrderDetails(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrderDetail>();

        entity.ToTable("OrderDetails", table =>
        {
            table.HasCheckConstraint("CK_OrderDetails_ProductNameAtPurchase_NotBlank", "LEN(LTRIM(RTRIM([ProductNameAtPurchase]))) > 0");
            table.HasCheckConstraint("CK_OrderDetails_Quantity_Positive", "[Quantity] > 0");
            table.HasCheckConstraint("CK_OrderDetails_UnitPrice_NonNegative", "[UnitPrice] >= 0");
        });

        entity.HasKey(detail => detail.OrderDetailId);
        entity.Property(detail => detail.OrderDetailId).UseIdentityColumn();
        entity.Property(detail => detail.ProductNameAtPurchase).HasMaxLength(200).IsRequired();
        entity.Property(detail => detail.UnitPrice).HasPrecision(18, 2);

        entity.HasIndex(detail => new { detail.OrderId, detail.ProductId })
            .IsUnique()
            .HasDatabaseName("UX_OrderDetails_OrderId_ProductId");

        entity.HasOne(detail => detail.Order)
            .WithMany(order => order.OrderDetails)
            .HasForeignKey(detail => detail.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(detail => detail.Product)
            .WithMany(product => product.OrderDetails)
            .HasForeignKey(detail => detail.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
