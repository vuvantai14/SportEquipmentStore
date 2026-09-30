# S14 - Shopping Cart

## Kiến trúc

Shopping Cart dùng luồng `CartController/ViewComponent → ICartService → CartService → EF Core → SQL Server`. Controller và Layout không truy cập DbContext.

## Customer resolution và bảo mật

- Tất cả route Cart được bảo vệ cho role Customer.
- `CustomerId` chỉ được lấy từ authenticated claim phía server, không nhận từ form/query.
- Các thao tác Add, Update, Remove và Clear dùng POST, anti-forgery token và PRG.
- Client chỉ gửi ProductId/Quantity; giá và tổng tiền được lấy/tính lại từ dữ liệu service.

## Chức năng

- Product Detail cho phép Customer thêm số lượng hợp lệ vào Cart; Guest được chuyển đến Login với local return URL.
- Cart page hiển thị ảnh, sản phẩm, giá hiện tại, số lượng, thành tiền và tổng tiền.
- Update, Remove và Clear gọi trực tiếp các method tương ứng của `ICartService`.
- Header dùng `CartSummaryViewComponent` để hiển thị tổng Quantity thật; Guest hiển thị 0.
- Empty state liên kết về Product Catalog; Checkout được hiển thị disabled và chưa có route.

## Stock validation

`CartService` tiếp tục kiểm tra Product/Category active, Quantity dương, tồn kho và tổng số lượng sau khi cộng item trùng. Lỗi nghiệp vụ được chuyển thành thông báo thân thiện.

## Database impact

Không thay đổi Entity, schema, migration hoặc seed. Dữ liệu Cart/CartItem phát sinh khi Customer sử dụng hệ thống là dữ liệu nghiệp vụ thông thường.

## Deferred

Checkout, Payment, Order History và Admin UI chưa được triển khai.
