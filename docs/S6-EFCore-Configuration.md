# S6 - Cấu hình EF Core và SQL Server

## 1. Phạm vi và căn cứ

S6 triển khai **ánh xạ dữ liệu**, chưa tạo schema hay kết nối thử đến database. [S4 - Thiết kế cơ sở dữ liệu](S4-Database-Design.md) là nguồn chuẩn cho 9 bảng và ràng buộc; 9 entity của S5 trong `SportEquipmentStore.Core` có đủ 53 thuộc tính scalar theo S4. Không sửa hoặc tạo lại entity. Quyết định nghiệp vụ BR14–BR25 được chốt ở cuối tài liệu S4 trong S6. Yêu cầu chức năng vẫn tham chiếu [S2](S2-System-Analysis.md) và [S3](S3-Use-Cases.md).

Tham chiếu project không đổi: Data → Core; Web → Core, Data; Admin → Core, Data. Core không tham chiếu EF Core, Data hoặc UI; không có phụ thuộc vòng. Data là tầng ánh xạ dùng chung cho Web và Admin về sau, nhưng S6 chỉ đăng ký DbContext trong Web, chưa xây chức năng Admin.

## 2. Package, DbContext và DbSet

`SportEquipmentStore.Data` tham chiếu `Microsoft.EntityFrameworkCore.SqlServer` **9.0.20** và `Microsoft.EntityFrameworkCore.Design` **9.0.20**. Package Design dùng `PrivateAssets=all`, không truyền phụ thuộc thiết kế sang project tiêu thụ. Target Framework giữ nguyên .NET 9.

`Data/Context/SportEquipmentStoreDbContext.cs` kế thừa `DbContext`, nhận `DbContextOptions<SportEquipmentStoreDbContext>` qua constructor và cấu hình Fluent API trong `OnModelCreating`. Chín DbSet là `Roles`, `Users`, `Customers`, `Categories`, `Products`, `Carts`, `CartItems`, `Orders`, `OrderDetails`. Tên bảng SQL trùng tên DbSet; không thêm bảng/entity khác.

## 3. Khóa và quan hệ

Mỗi bảng dùng khóa chính một cột `int IDENTITY(1,1)` đúng S4; không tạo composite PK hoặc shadow FK. Mọi FK dưới đây là `NOT NULL` vì thuộc tính FK trong S5 là `int`. Tất cả dùng `DeleteBehavior.NoAction` (`ON DELETE NO ACTION`) để không xóa dây chuyền dữ liệu lịch sử.

| Quan hệ | Cardinality | FK |
|---|---|---|
| Roles → Users | 1 → 0..N | `Users.RoleId` → `Roles.RoleId` |
| Users → Customers | 1 → 0..1 | `Customers.UserId` → `Users.UserId`, unique |
| Categories → Products | 1 → 0..N | `Products.CategoryId` → `Categories.CategoryId` |
| Customers → Carts | 1 → 0..1 | `Carts.CustomerId` → `Customers.CustomerId`, unique |
| Carts → CartItems | 1 → 0..N | `CartItems.CartId` → `Carts.CartId` |
| Products → CartItems | 1 → 0..N | `CartItems.ProductId` → `Products.ProductId` |
| Customers → Orders | 1 → 0..N | `Orders.CustomerId` → `Customers.CustomerId` |
| Orders → OrderDetails | 1 → 0..N tại mức FK; 1..N theo nghiệp vụ | `OrderDetails.OrderId` → `Orders.OrderId` |
| Products → OrderDetails | 1 → 0..N | `OrderDetails.ProductId` → `Products.ProductId` |

Nhờ NO ACTION, không thể vô ý xóa Category còn Product, Product còn OrderDetail/CartItem, Customer còn Order, hoặc User còn Customer. Category/Product/User có `IsActive` để ngừng sử dụng thay cho xóa cứng khi cần giữ liên kết. Xóa CartItem hoặc dữ liệu đủ điều kiện khác cần thao tác tường minh ở lớp nghiệp vụ sau này.

## 4. Ràng buộc cột và chỉ mục

Required/optional và độ dài chuỗi được ánh xạ theo mục 3 của S4: `RoleName(30)`, `Username(50)`, `Email(254)`, `PasswordHash(512)`, `FullName(150)`, `Customer.Phone(20)`, `Customer.Address(500)`, `CategoryName(120)`, `Category.Description(1000)`, `ProductName(200)`, `Product.Description(2000)`, `ImageUrl(2048)`, `ProductNameAtPurchase(200)`, `ShippingFullName(150)`, `ShippingPhone(20)`, `ShippingAddress(500)`, `Note(1000)`. Ba cột tiền `Products.Price`, `Orders.TotalAmount`, `OrderDetails.UnitPrice` là `decimal(18,2)`. `StockQuantity` mặc định 0; các `IsActive` mặc định true; `CreatedAt`/`OrderDate` là `datetime2(3)` với mặc định `SYSUTCDATETIME()`; `Products.RowVersion` và `Orders.RowVersion` là token `rowversion` do SQL Server sinh.

`Orders.Status` là enum C# `OrderStatus` được chuyển sang `nvarchar(20)`, mặc định `Pending`, collation `Latin1_General_100_BIN2` để CHECK chỉ nhận đúng `Pending`, `Confirmed`, `Shipping`, `Completed`, `Cancelled`. `Username`/`Email` dùng `Latin1_General_100_CI_AS` để unique không phân biệt hoa/thường và phân biệt dấu. CHECK yêu cầu username/email đã trim, không rỗng; tên, hash và thông tin giao hàng không toàn khoảng trắng. CHECK khác chặn Price/StockQuantity/TotalAmount/UnitPrice âm và Quantity của CartItem/OrderDetail không dương. Các CHECK này chỉ có hiệu lực khi schema được tạo ở bước sau; validation đầu vào vẫn phải thực hiện trong ứng dụng.

Unique indexes: `Roles.RoleName`, `Users.Username`, `Users.Email`, `Customers.UserId`, `Carts.CustomerId`, `(CartItems.CartId, ProductId)`, `(Orders.CustomerId, CheckoutRequestId)`, `(OrderDetails.OrderId, ProductId)`. Khi thêm lại cùng Product vào Cart, service tương lai cập nhật Quantity của dòng cũ thay vì tạo dòng trùng. Không áp đặt unique lên `CategoryName` vì S4 chưa chốt chính sách trùng tên.

Các index truy vấn không unique theo S4: `Products(CategoryId, IsActive)`, `Products(ProductName)`, `Orders(CustomerId, OrderDate DESC)`, `Orders(Status, OrderDate DESC)`, `Orders(OrderDate DESC)`. Không tạo thêm index cho mọi FK; sau khi có dữ liệu thực cần đo truy vấn. Index B-tree theo ProductName không bảo đảm tăng tốc tìm chứa từ dạng `LIKE '%...%'`.

## 5. Kết nối và đăng ký Web

`SportEquipmentStore.Web/appsettings.json` có `ConnectionStrings:DefaultConnection` trỏ đến `(localdb)\MSSQLLocalDB`, database `SportEquipmentStoreDb`, Windows Trusted Connection, MARS và `TrustServerCertificate=True`. Đây là cấu hình phát triển **không chứa username/password hay secret**. `TrustServerCertificate=True` chỉ phù hợp môi trường phát triển; môi trường khác phải dùng cấu hình riêng, bảo vệ bí mật và chứng chỉ phù hợp. Web `Program.cs` đọc `DefaultConnection`, báo lỗi rõ nếu thiếu và đăng ký `AddDbContext<SportEquipmentStoreDbContext>(options => options.UseSqlServer(connectionString))`. Pipeline Web khác giữ nguyên.

LocalDB đã được phát hiện trên máy nhưng automatic instance `MSSQLLocalDB` chưa được tạo tại thời điểm kiểm tra. S6 không mở kết nối, tạo instance, tạo database, chạy migration hay seed; vì vậy **chưa xác nhận kết nối thực tế**. Admin sẽ dùng cùng database qua Data trong giai đoạn sau; S6 không đăng ký kết nối trong WinForms.

## 6. Quyết định nghiệp vụ đã chốt và giới hạn ánh xạ

| Mã S4/S6 | Quyết định |
|---|---|
| BR14–BR15 | Trừ tồn khi tạo Order thành công; hoàn tồn đúng một lần nếu Order đã trừ tồn rồi bị hủy. |
| BR16–BR17 | Customer chỉ hủy đơn Pending của mình; Admin hủy Pending hoặc Confirmed, không hủy Shipping/Completed. |
| BR18 | Đăng nhập chính bằng Username; Email vẫn unique. |
| BR19–BR20 | Category có Product và Product đã vào OrderDetail không xóa cứng; dùng IsActive khi phù hợp. |
| BR21 | Doanh thu chỉ lấy Order Completed. |
| BR22–BR23 | UnitPrice và TotalAmount là snapshot lúc đặt; tổng bằng `SUM(Quantity × UnitPrice)` của OrderDetails, không lấy Product.Price hiện hành. |
| BR24 | Cart/CartItems tồn tại lâu dài trong SQL Server; giỏ không giữ hàng hoặc chốt giá. |
| BR25 | Thanh toán bản cơ bản là COD; cổng thanh toán trực tuyến chỉ là extension. |

Mã BR14–BR16 ở đây là của **S4/S6**, khác BR14–BR16 trong S2; khi viện dẫn phải ghi nguồn. Các quyết định trên **chưa được thực thi** bằng service: giao dịch tạo đơn/trừ tồn và hoàn tồn một lần, đơn phải có ít nhất một OrderDetail, tính TotalAmount từ chi tiết, chống đặt trùng ở tầng xử lý, kiểm tra role Customer, quyền hủy/chuyển trạng thái, active của Product/Category và doanh thu báo cáo đều cần code nghiệp vụ sau S6. FK, CHECK, unique và RowVersion chỉ cung cấp lớp bảo vệ dữ liệu tương ứng, không thay thế các quy tắc liên bảng hoặc kiểm tra quyền.

Những điểm S4 chưa chốt như xử lý Product active khi Category bị ẩn, chính sách tên danh mục trùng, giá 0, lưu ảnh và thu hồi phiên inactive vẫn mở; S6 không tự đổi mô hình để giả định câu trả lời. Không có khác biệt giữa 53 thuộc tính scalar của S5 và 53 cột S4; không sửa entity S5.

## 7. Xác minh và ranh giới S6

Đã chạy `dotnet restore SportEquipmentStore.sln` và `dotnet build SportEquipmentStore.sln --no-restore`: đều thành công, build 0 warning/0 error. Build chỉ xác nhận mã biên dịch, không chứng minh SQL Server hoặc schema chạy đúng. S6 **không** tạo migration, database, seed data, repository/service, login/authorization, CRUD, checkout, dashboard, Web UI hay WinForms UI. S7 chưa bắt đầu.
