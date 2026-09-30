# Sport Equipment Store

## Project Overview

**Đề tài:** Xây dựng hệ thống quản lý và bán dụng cụ thể thao.

**Môn:** Ngôn ngữ lập trình C#

Sport Equipment Store gồm website ASP.NET Core MVC dành cho khách hàng và ứng dụng Windows Forms dành cho quản trị viên. Hai ứng dụng chia sẻ Core, Service Layer và dữ liệu nghiệp vụ trên Microsoft SQL Server.

## Features

### Customer Web

- Responsive customer website.
- Shared header, navigation và footer.
- Responsive mobile navigation.
- Database-driven category navigation.
- Customer homepage với Hero và trust benefits.
- Featured categories và product showcase từ database.
- Product Catalog tại `/products`.
- Product Detail tại `/products/{id}`.
- Search backend từ Header và Product Catalog.
- Customer Register, Login và Logout bằng cookie authentication.
- Password hashing và trạng thái đăng nhập trên Header.
- Shopping Cart cho Customer với Add, Update, Remove và Clear.
- Cart badge hiển thị tổng số lượng từ database.
- Lọc sản phẩm theo danh mục và sắp xếp theo giá hoặc tên.
- Reusable Product Card với trạng thái tồn kho.
- Local product image fallback và SVG assets.
- Empty states cho dữ liệu không có kết quả.
- Basic accessibility support với semantic markup, labels và focus states.

### Business Services

- Category management.
- Product management và search service.
- Customer management.
- Persistent cart business logic.
- Cart quantity và stock validation.
- Order creation từ giỏ hàng.
- Transaction handling cho quá trình tạo đơn và cập nhật trạng thái.
- Inventory deduction và restoration.
- Order status và cancellation rules.
- Order total calculation tại Service Layer.
- Customer authentication service.

### Admin Application

- Windows Forms project foundation.
- Shared Core/Data architecture.
- Được thiết kế sử dụng chung SQL Server database với Customer Web.

Admin Login, Dashboard và các màn hình quản lý CRUD chưa được triển khai.

## Technology Stack

- C# và .NET 9
- ASP.NET Core MVC
- Razor Views
- Windows Forms
- Entity Framework Core 9.0.20
- Microsoft SQL Server
- HTML, CSS, Bootstrap và JavaScript
- Git và GitHub

## Architecture

```text
Customer Web / Admin WinForms
            │
            ▼
       Service Layer
            │
            ▼
   Entity Framework Core
            │
            ▼
   Microsoft SQL Server
```

- `SportEquipmentStore.Core`: Entities, Enums, Interfaces và shared Models.
- `SportEquipmentStore.Data`: DbContext, Migrations, Seed và Services.
- `SportEquipmentStore.Web`: ASP.NET Core MVC Customer Web.
- `SportEquipmentStore.Admin`: Windows Forms Admin foundation.

Service Layer tập trung business rules để Web và Admin có thể dùng chung dữ liệu nghiệp vụ mà không đặt logic truy cập dữ liệu trong UI.

## Project Structure

```text
SportEquipmentStore/
├── docs/
├── src/
│   ├── SportEquipmentStore.Admin/
│   ├── SportEquipmentStore.Core/
│   ├── SportEquipmentStore.Data/
│   └── SportEquipmentStore.Web/
├── SportEquipmentStore.sln
└── README.md
```

## Database

- **Database:** `SportEquipmentStoreDb`
- **DBMS:** Microsoft SQL Server
- **Development authentication:** Windows Authentication

Business tables:

- `Roles`
- `Users`
- `Customers`
- `Categories`
- `Products`
- `Carts`
- `CartItems`
- `Orders`
- `OrderDetails`

Seed data hiện tại gồm 2 Roles, 6 Categories và 12 Products. Seed không tạo tài khoản quản trị hoặc mật khẩu mẫu.

## Service Layer

- `ICategoryService` / `CategoryService`
- `IProductService` / `ProductService`
- `ICustomerService` / `CustomerService`
- `ICartService` / `CartService`
- `IOrderService` / `OrderService`
- `IAuthService` / `AuthService`

Luồng xử lý chung:

```text
UI → Service Layer → Entity Framework Core → SQL Server
```

## Product Catalog

- Products được tải từ SQL Server thông qua Service Layer.
- Chỉ hiển thị Product và Category đang active.
- Lọc theo danh mục bằng GET/query string.
- Sắp xếp giá tăng dần, giá giảm dần và tên A–Z.
- Tái sử dụng Product Card từ Homepage.
- Hiển thị giá, danh mục và trạng thái tồn kho.
- Dùng local placeholder khi thiếu ảnh.
- Xử lý empty state và category filter không hợp lệ.
- Product Card mở trang chi tiết bằng ASP.NET Core routing.

## Business Rules

- Category và Product sử dụng `IsActive`.
- Customer Web chỉ hiển thị Product khi cả Product và Category đều active.
- Cart và CartItem được lưu trong database.
- Số lượng trong Cart phải lớn hơn 0 và không vượt tồn kho.
- `OrderDetail.UnitPrice` lưu giá tại thời điểm đặt hàng.
- `Order.TotalAmount` được tính từ OrderDetails tại Service Layer.
- Tạo Order, OrderDetails, trừ tồn kho và xóa CartItems được thực hiện trong transaction.
- Stock chỉ giảm khi tạo Order thành công.
- Hủy Order hợp lệ hoàn tồn kho theo business rule và không hoàn lặp lại.
- Luồng trạng thái hỗ trợ `Pending → Confirmed → Shipping → Completed`; `Cancelled` chỉ áp dụng từ trạng thái hợp lệ.

## Getting Started

Yêu cầu: .NET 9 SDK, Microsoft SQL Server và EF Core CLI tools.

Khôi phục dependencies và build solution:

```powershell
dotnet restore
dotnet build
```

Cập nhật database từ migration hiện có:

```powershell
dotnet ef database update --project src/SportEquipmentStore.Data --startup-project src/SportEquipmentStore.Web
```

Chạy Customer Web:

```powershell
dotnet run --project src/SportEquipmentStore.Web
```

Không lưu connection string chứa thông tin bí mật trong source control. Nếu cấu hình SQL Server cục bộ khác cấu hình mặc định, hãy dùng biến môi trường, user secrets hoặc file cấu hình không được commit.

## Documentation

- [System Analysis](docs/S2-System-Analysis.md)
- [Use Cases](docs/S3-Use-Cases.md)
- [Database Design](docs/S4-Database-Design.md)
- [EF Core Configuration](docs/S6-EFCore-Configuration.md)
- [Database Migration & Seed](docs/S7-Database-Migration-Seed.md)
- [Service Layer](docs/S8-Service-Layer.md)
- [Web UI Foundation](docs/S9-Web-UI-Foundation.md)
- [Customer Homepage](docs/S10-Customer-Homepage.md)
- [Product Catalog](docs/S11-Product-Catalog.md)
- [Product Detail & Search](docs/S12-Product-Detail-Search.md)
- [Customer Authentication](docs/S13-Customer-Authentication.md)
- [Shopping Cart](docs/S14-Shopping-Cart.md)

## Current Status

### Available

- Solution architecture và domain entities.
- SQL Server database, EF Core migrations và seed data.
- Shared Service Layer.
- Customer Web foundation và responsive Homepage.
- Database-driven categories và products.
- Product Catalog, Product Detail, search, category filtering và product sorting.
- Customer registration, login, logout và cookie authentication.
- Customer Shopping Cart và database-driven cart badge.

### Under Development

- Checkout.
- Order History.
- Admin management UI.

## Security

- Không commit password, database credential hoặc secret.
- Customer password được hash bằng ASP.NET Core `PasswordHasher<User>`; không lưu plain text.
- Cấu hình development hiện tại sử dụng Windows Authentication cho SQL Server.
- Production secrets phải được lưu ngoài source control.
