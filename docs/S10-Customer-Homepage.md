# S10 - Customer Homepage

## Mục tiêu

Xây dựng trang chủ khách hàng hoàn chỉnh, responsive và đồng bộ với UI foundation hiện có. Homepage hiển thị Category và Product thật từ `SportEquipmentStoreDb`, không tạo dữ liệu nghiệp vụ giả và không thay đổi database.

## Data flow

```text
HomeController
    ↓
ICategoryService / IProductService
    ↓
Entity Framework Core
    ↓
Microsoft SQL Server
```

`HomeController` không truy cập `DbContext`. Hai service được inject qua dependency injection và `Index` dùng async với `CancellationToken`.

## HomeViewModel

`HomeViewModel` là model strongly typed của `Views/Home/Index.cshtml`, gồm:

- `FeaturedCategories`: danh sách tối đa 6 `HomeCategoryViewModel`.
- `FeaturedProducts`: danh sách tối đa 8 `ProductCardViewModel`.

Entity chỉ được đọc trong Controller để map sang ViewModel; View không thực hiện query.

## Nguồn Category và Product

- Category lấy từ `ICategoryService.GetActiveAsync`, đã được service sắp xếp ổn định theo tên và chỉ lấy 6 dòng đầu.
- Product lấy từ `IProductService.GetActiveAsync`, đã được service sắp xếp ổn định theo tên và chỉ lấy 8 dòng đầu.
- Query Product của service yêu cầu cả `Product.IsActive` và `Category.IsActive`.
- Product card hiển thị tên, category, giá, tồn kho và ảnh từ dữ liệu trả về.
- Khi `ImageUrl` thiếu, `_ProductCard.cshtml` sử dụng local placeholder.

“Sản phẩm nổi bật” hiện chỉ là tập nhỏ active products dùng để giới thiệu trên Home; database chưa có `IsFeatured`.

## Cấu trúc Homepage

1. Hero với hai CTA nội trang, không điều hướng tới route chưa tồn tại.
2. Trust Benefits tái sử dụng `_TrustBenefits.cshtml`.
3. Danh mục nổi bật lấy từ database.
4. Sản phẩm nổi bật tái sử dụng `_ProductCard.cshtml`.
5. CTA cuối trang quay lại section sản phẩm.

Navigation và footer dùng anchor tới các section Homepage cho Sản phẩm/Danh mục. Product detail vẫn ở trạng thái disabled.

## Responsive

- 1440 px: Category 3 cột, Product 4 cột.
- 768 px: Category 2 cột, Product 2 cột.
- 375 px: Category và Product 1 cột; CTA full-width.
- Hero chuyển một cột trên mobile.
- Không có horizontal overflow tại ba viewport kiểm thử.
- CSS riêng của trang nằm tại `wwwroot/css/pages/home.css`.

## Empty states

Category và Product đều có empty state riêng. Các state này hiển thị thông báo an toàn khi service trả danh sách rỗng, không tạo dữ liệu mẫu thay thế và không dẫn tới route giả.

## Testing

- Restore solution: PASS.
- Release build toàn solution: PASS, 0 error, 0 warning.
- Home: HTTP 200 tại URL kiểm thử `http://127.0.0.1:5219`.
- CSS, JavaScript, SVG icons và product placeholder: HTTP 200.
- Dữ liệu runtime: 6 Category card và 8 Product card.
- SQL log xác nhận Category active và Product active thuộc Category active được query từ SQL Server.
- Chrome DevTools emulation 1440/768/375: không overflow; ảnh tải đủ; Bootstrap và mobile menu hoạt động; không có browser exception/network failure.

Build Debug ban đầu bị một tiến trình Web do môi trường phát triển giữ file executable. Không dừng tiến trình đó; cấu hình Release độc lập được dùng cho kết quả build và runtime chính thức.

## Chức năng để lại bước sau

- Product Catalog, filter, sort và pagination.
- Product Detail và Search backend.
- Login, Register và Authentication.
- Cart workflow, Checkout và Payment.
- Order History.
- Admin UI.
