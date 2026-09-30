# S15 - Customer Checkout

## Checkout flow

Checkout dùng luồng `CheckoutController → IOrderService → OrderService → EF Core → SQL Server`. CustomerId được lấy từ authenticated claim; controller không truy cập DbContext và không tự tạo Order/OrderDetail.

GET `/checkout` kiểm tra Cart, lấy hồ sơ Customer để điền sẵn thông tin người nhận và tạo `CheckoutRequestId`. POST validate ViewModel rồi gọi `CreateOrderFromCartAsync`; thành công chuyển theo PRG tới `/checkout/success/{id}`.

## Cart và Order

- Cart rỗng bị chặn trước khi hiển thị hoặc tạo đơn.
- Order summary được tải server-side; form không gửi giá, tổng tiền, stock hoặc trạng thái.
- `OrderService` kiểm tra lại Product/Category, quantity và stock khi submit.
- Service tạo Order/OrderDetails, snapshot ProductName/UnitPrice, tính TotalAmount, đặt trạng thái Pending, trừ stock và clear Cart.
- Các thay đổi được thực hiện trong một Serializable transaction; lỗi rollback toàn bộ.

## Idempotency và Success

`CheckoutRequestId` hiện có được giữ qua form và unique theo Customer. Request lặp lại cùng ID trả về Order đã tạo thay vì tạo đơn thứ hai.

Success page lấy Order qua service, kiểm tra `Order.CustomerId` trùng Customer claim rồi mới hiển thị mã đơn, ngày đặt, trạng thái, tổng tiền, chi tiết và thông tin giao hàng. Cart badge trở về 0 sau khi service clear Cart.

## Security và database impact

Checkout yêu cầu role Customer, POST có anti-forgery, dùng strongly typed ViewModel và xử lý lỗi nghiệp vụ thân thiện. Không thay đổi Entity, schema, migration hoặc seed.

## Deferred

Payment Gateway, Order History, Customer Cancellation UI, Admin Order Management và Admin UI chưa được triển khai.
