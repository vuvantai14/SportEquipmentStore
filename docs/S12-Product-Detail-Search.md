# S12 - Product Detail và Search

## Mục tiêu

Hoàn thiện trang chi tiết sản phẩm và kết nối tìm kiếm Customer Web với Product Catalog hiện có.

## Luồng xử lý

- Product Detail dùng route `/products/{id}`, gọi `IProductService.GetByIdAsync` và trả 404 khi ID không tồn tại, Product inactive hoặc Category inactive.
- Search dùng GET `/products?q=...` và gọi `IProductService.SearchAsync`.
- Catalog xử lý theo thứ tự: tập sản phẩm active → search → category filter → sort → ViewModel.
- Search, category và sort nằm trong cùng form nên query state được giữ khi áp dụng bộ lọc.

## ViewModels và giao diện

- `ProductDetailViewModel` chứa đúng dữ liệu hiện có của Product và Category.
- `ProductCatalogViewModel` được bổ sung `SearchQuery`.
- Product Card dùng route helper để mở trang chi tiết từ Homepage và Product Catalog.
- Product Detail dùng ảnh placeholder cục bộ khi thiếu ảnh và không có chức năng Add to Cart.

## Service Layer và database safety

Controller chỉ sử dụng Service Layer, không truy cập DbContext. Không thay đổi Entity, schema, migration hoặc seed data.

## Deferred

Authentication, Cart UI, Checkout, Order History, Payment, Review, Rating, Wishlist và Admin UI chưa được triển khai.
