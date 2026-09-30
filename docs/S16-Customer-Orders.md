# Customer Order History, Detail and Cancellation

## Luồng chức năng

- `GET /orders` yêu cầu role Customer, lấy CustomerId từ claim và gọi `IOrderService.GetByCustomerAsync`. Danh sách sắp xếp mới nhất trước và có empty state.
- `GET /orders/{id}` gọi `IOrderService.GetByIdAsync`, sau đó kiểm tra Order thuộc Customer đang đăng nhập. Đơn không tồn tại hoặc thuộc Customer khác đều trả về HTTP 404.
- `POST /orders/{id}/cancel` có anti-forgery, xác minh ownership rồi gọi `IOrderService.CancelByCustomerAsync`. Controller không tự cập nhật trạng thái hoặc tồn kho.

## Dữ liệu hiển thị

History và Detail dùng strongly typed ViewModel. Chi tiết sản phẩm lấy `ProductNameAtPurchase` và `UnitPrice` đã snapshot trong OrderDetail, không dùng tên hoặc giá hiện tại của Product. Tổng tiền hiển thị từ Order và tổng từng dòng được tính từ snapshot `UnitPrice × Quantity`.

## Quy tắc hủy và bảo mật

Customer chỉ được hủy Order ở trạng thái Pending. Service kiểm tra lại ownership và trạng thái trong transaction, chuyển sang Cancelled, hoàn tồn kho đúng một lần và xử lý lời gọi lặp an toàn. Nút hủy chỉ hiện khi hợp lệ; POST không nhận CustomerId hoặc OrderStatus từ client.

Các lỗi do trạng thái thay đổi đồng thời được chuyển thành thông báo thân thiện. Không có DbContext trong controller hoặc view, và Layout không truy vấn database.

## UI và phạm vi

Giao diện gồm lịch sử đơn, empty state, trang chi tiết, badge trạng thái, thông tin giao hàng và thao tác hủy responsive ở 1440px, 768px và 375px. Header hiển thị “Đơn hàng của tôi” cho Customer đã đăng nhập.

Không thay đổi Entity, schema, migration, seed hoặc Service Layer. Payment Gateway, tracking realtime, reorder, invoice, review, return/refund và Admin Order Management được để lại cho giai đoạn sau.
