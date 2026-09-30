# S7 - Initial Migration, SQL Server và Seed Data

## 1. Môi trường và trạng thái

- SQL Server được yêu cầu: `TAIIII\SERVER`
- Database được yêu cầu: `SportEquipmentStoreDb`
- Authentication: Windows Authentication
- Entity Framework Core runtime/packages: `9.0.20`
- EF Core CLI phát hiện trên máy: `9.0.4`
- Migration: `InitialCreate`
- Vị trí: `src/SportEquipmentStore.Data/Migrations`

`DefaultConnection` trong Web đã được chuyển khỏi LocalDB sang:

```text
Server=TAIIII\SERVER;Database=SportEquipmentStoreDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Chuỗi kết nối không chứa SQL username, password hay secret và không được hard-code trong DbContext.

Kết nối thử bằng Windows Authentication tới đúng `TAIIII\SERVER` **không thành công**: SQL client báo không tìm thấy server/instance và hết thời gian đăng nhập. Dịch vụ `SQL Server (SERVER)` trên máy đang chạy, nhưng hostname hệ điều hành trả về là `taiiiii`, không trùng với hostname `TAIIII` được giao. Theo điều kiện an toàn của S7, không đổi sang LocalDB, SQL Express hoặc instance khác và không chạy `database update`. Vì vậy database/schema/seed vật lý chưa được tạo hay kiểm chứng.

## 2. Migration và database model

Migration `20260929234414_InitialCreate` tạo đúng 9 bảng nghiệp vụ:

1. `Roles`
2. `Users`
3. `Customers`
4. `Categories`
5. `Products`
6. `Carts`
7. `CartItems`
8. `Orders`
9. `OrderDetails`

Khi migration được áp dụng, EF Core còn tạo `__EFMigrationsHistory`; đây không phải bảng nghiệp vụ.

Migration có 9 khóa ngoại, đều dùng `NO ACTION`:

| Quan hệ | FK/Cardinality |
|---|---|
| Role → Users | `Users.RoleId`, 1 → 0..N |
| User → Customer | `Customers.UserId` unique, 1 → 0..1 |
| Category → Products | `Products.CategoryId`, 1 → 0..N |
| Customer → Cart | `Carts.CustomerId` unique, 1 → 0..1 |
| Cart → CartItems | `CartItems.CartId`, 1 → 0..N |
| Product → CartItems | `CartItems.ProductId`, 1 → 0..N |
| Customer → Orders | `Orders.CustomerId`, 1 → 0..N |
| Order → OrderDetails | `OrderDetails.OrderId`, 1 → N theo nghiệp vụ |
| Product → OrderDetails | `OrderDetails.ProductId`, 1 → 0..N |

Đã đọc migration và xác nhận:

- 9 lệnh tạo bảng, 9 FK, không có `ReferentialAction.Cascade`.
- PK là `int IDENTITY(1,1)`; không phát sinh composite PK hoặc shadow FK.
- Unique indexes cho RoleName, Username, Email, User–Customer, Customer–Cart, Cart–Product, Customer–CheckoutRequest và Order–Product.
- Index truy vấn theo ProductName, Category/IsActive và các tổ hợp OrderDate/Customer/Status; EF bổ sung index FK cần thiết cho các chiều tra cứu còn lại.
- `Products.Price`, `Orders.TotalAmount`, `OrderDetails.UnitPrice` là `decimal(18,2)`.
- CHECK constraints, max length, trạng thái Order, collation, UTC defaults và rowversion khớp cấu hình S6.
- `dotnet ef migrations has-pending-model-changes` xác nhận không còn thay đổi model sau `InitialCreate`.

## 3. Seed Data

Seed dùng `HasData` với ID và `CreatedAt` cố định, nên migration có dữ liệu deterministic:

- Roles: 2 bản ghi — `Admin`, `Customer`.
- Categories: 6 bản ghi — Bóng đá, Cầu lông, Bóng rổ, Tennis, Gym & Fitness, Bơi lội.
- Products: 12 bản ghi hợp lệ, phân bổ trên cả 6 danh mục; giá dương, tồn kho không âm, active và không phụ thuộc URL ảnh bên ngoài.
- Customers, Carts, CartItems, Orders, OrderDetails: không seed.
- Admin account: **không seed**. Dự án chưa có AuthenticationService hoặc cơ chế băm mật khẩu chuẩn; không ghi plain text hoặc chuỗi giả vào `PasswordHash`.

Các số lượng trên đã được xác nhận trong source migration, chưa thể xác nhận bằng truy vấn database vì kết nối thất bại và migration chưa được áp dụng.

## 4. Category inactive rule

S4 đã bổ sung BR26:

- `Category.IsActive = false` không tự đổi `Product.IsActive`.
- Product giữ trạng thái riêng.
- Web chỉ hiển thị Product khi `Product.IsActive == true && Product.Category.IsActive == true`.
- Admin vẫn xem được Product thuộc Category inactive.
- Không hard delete Category chỉ để ẩn khỏi Web.

Đây là quy tắc cho Service Layer/Web sau S7; S7 không triển khai ProductService.

## 5. Các lệnh đã dùng

Kiểm tra công cụ và tạo migration:

```powershell
dotnet ef --version
dotnet ef migrations add InitialCreate --project src/SportEquipmentStore.Data --startup-project src/SportEquipmentStore.Web --output-dir Migrations
dotnet ef migrations has-pending-model-changes --project src/SportEquipmentStore.Data --startup-project src/SportEquipmentStore.Web
```

Lệnh áp dụng database dự kiến là:

```powershell
dotnet ef database update --project src/SportEquipmentStore.Data --startup-project src/SportEquipmentStore.Web
```

Lệnh này **chưa được chạy**. Chỉ chạy sau khi `TAIIII\SERVER` kết nối PASS bằng Windows Authentication. Sau lần đầu thành công, chạy lại cùng lệnh để xác nhận thông báo database up-to-date và seed không bị nhân đôi.

## 6. Kiểm tra bằng SSMS sau khi kết nối được sửa

1. Mở SSMS, chọn Database Engine, Server name `TAIIII\SERVER`, Authentication `Windows Authentication`.
2. Kiểm tra kết nối thành công trước khi chạy migration.
3. Sau `database update`, refresh Databases và mở `SportEquipmentStoreDb > Tables`.
4. Xác nhận 9 bảng nghiệp vụ cùng `dbo.__EFMigrationsHistory`.
5. Chạy truy vấn:

```sql
USE SportEquipmentStoreDb;

SELECT COUNT(*) AS RoleCount FROM dbo.Roles;
SELECT COUNT(*) AS CategoryCount FROM dbo.Categories;
SELECT COUNT(*) AS ProductCount FROM dbo.Products;

SELECT p.ProductId, p.ProductName, p.CategoryId, c.CategoryName
FROM dbo.Products AS p
INNER JOIN dbo.Categories AS c ON c.CategoryId = p.CategoryId
ORDER BY p.ProductId;

SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory;
```

Kết quả mong đợi sau khi áp dụng: RoleCount = 2, CategoryCount = 6, ProductCount = 12; mọi Product nối được với một Category và lịch sử có migration `InitialCreate`.

## 7. Vấn đề gặp phải và xử lý

- Kết nối `TAIIII\SERVER` thất bại do không định vị được server/instance. Database update, kiểm tra bảng, seed và lần update thứ hai đều được dừng; không chuyển sang instance khác.
- EF CLI lần đầu không tạo migration vì startup Web chưa tham chiếu trực tiếp `Microsoft.EntityFrameworkCore.Design`. Đã thêm package 9.0.20 với `PrivateAssets=all` vào Web; package trong Data vẫn giữ nguyên.
- EF CLI 9.0.4 chạy được với runtime 9.0.20 nhưng cảnh báo tool cũ hơn runtime. Không tự ý thay đổi global tool; có thể nâng dotnet-ef lên 9.0.20 sau khi thống nhất quản lý tool.
- Một lần kiểm tra snapshot với `--no-build` được chạy trước khi assembly chứa migration mới, khiến CLI báo sai rằng còn thay đổi. Không giữ migration chẩn đoán; sau khi build lại, kiểm tra chính thức trả về “No changes have been made to the model since the last migration”.

## 8. Phạm vi

S7 không triển khai repository/service, authentication/password hashing, controller, CRUD, checkout, Dashboard, Web UI, WinForms UI hoặc thanh toán trực tuyến. S8 chưa bắt đầu.
