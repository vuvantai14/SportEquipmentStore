using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;

namespace SportEquipmentStore.Data.Seed;

internal static class ModelSeedData
{
    private static readonly DateTime SeedCreatedAtUtc =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Customer" });

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Bóng đá", Description = "Dụng cụ và phụ kiện bóng đá.", IsActive = true },
            new Category { CategoryId = 2, CategoryName = "Cầu lông", Description = "Dụng cụ và phụ kiện cầu lông.", IsActive = true },
            new Category { CategoryId = 3, CategoryName = "Bóng rổ", Description = "Dụng cụ và phụ kiện bóng rổ.", IsActive = true },
            new Category { CategoryId = 4, CategoryName = "Tennis", Description = "Dụng cụ và phụ kiện tennis.", IsActive = true },
            new Category { CategoryId = 5, CategoryName = "Gym & Fitness", Description = "Thiết bị tập luyện thể hình và fitness.", IsActive = true },
            new Category { CategoryId = 6, CategoryName = "Bơi lội", Description = "Dụng cụ và phụ kiện bơi lội.", IsActive = true });

        modelBuilder.Entity<Product>().HasData(
            Product(1, "Bóng đá thi đấu", 1, 450_000m, 30, "Bóng đá kích thước số 5 dùng cho tập luyện và thi đấu."),
            Product(2, "Giày đá bóng", 1, 890_000m, 24, "Giày đá bóng đế bám phù hợp sân cỏ nhân tạo."),
            Product(3, "Găng tay thủ môn", 1, 350_000m, 18, "Găng tay thủ môn có lớp đệm và độ bám tốt."),
            Product(4, "Vợt cầu lông", 2, 650_000m, 25, "Vợt cầu lông cân bằng, phù hợp người chơi phong trào."),
            Product(5, "Cầu lông ống 12 quả", 2, 180_000m, 50, "Ống 12 quả cầu lông dùng cho tập luyện."),
            Product(6, "Bóng rổ thi đấu", 3, 520_000m, 20, "Bóng rổ kích thước số 7 với bề mặt chống trượt."),
            Product(7, "Giày bóng rổ", 3, 1_250_000m, 15, "Giày bóng rổ hỗ trợ cổ chân và giảm chấn."),
            Product(8, "Vợt tennis", 4, 1_450_000m, 12, "Vợt tennis trọng lượng trung bình cho người mới và bán chuyên."),
            Product(9, "Tạ tay 5kg", 5, 320_000m, 40, "Tạ tay bọc cao su khối lượng 5kg."),
            Product(10, "Thảm tập yoga", 5, 280_000m, 35, "Thảm tập chống trượt dùng cho yoga và fitness."),
            Product(11, "Kính bơi chống sương", 6, 220_000m, 45, "Kính bơi có lớp phủ hạn chế đọng sương."),
            Product(12, "Mũ bơi silicone", 6, 120_000m, 60, "Mũ bơi silicone co giãn và chống thấm tốt."));
    }

    private static Product Product(
        int productId,
        string productName,
        int categoryId,
        decimal price,
        int stockQuantity,
        string description)
    {
        return new Product
        {
            ProductId = productId,
            ProductName = productName,
            CategoryId = categoryId,
            Price = price,
            StockQuantity = stockQuantity,
            Description = description,
            ImageUrl = null,
            IsActive = true,
            CreatedAt = SeedCreatedAtUtc
        };
    }
}
