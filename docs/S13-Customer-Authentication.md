# S13 - Customer Authentication

## Kiến trúc

Customer authentication dùng luồng `AccountController → IAuthService → AuthService → EF Core → SQL Server`. Controller không truy cập DbContext; `AuthService` chịu trách nhiệm đăng ký, kiểm tra tài khoản và xử lý password hash.

## Register

- Form dùng `RegisterViewModel` và DataAnnotations.
- Username/Email được kiểm tra trùng bằng service và unique index hiện có.
- Role `Customer` được lấy từ database phía server; client không gửi RoleId.
- User và Customer được tạo trong một transaction.
- Password được hash bằng `PasswordHasher<User>` trước khi lưu.
- Đăng ký thành công chuyển đến trang Login.

## Login và cookie

- Cho phép đăng nhập bằng Username hoặc Email.
- Service chặn User inactive, tài khoản không có role Customer hoặc thiếu Customer profile.
- Password được xác minh bằng `PasswordHasher<User>`.
- Cookie authentication dùng cookie HttpOnly, SameSite Lax và SecurePolicy phù hợp request.
- Claims gồm UserId, Username, Customer role, CustomerId, Email và FullName; không chứa password/hash.
- `returnUrl` chỉ được dùng khi `Url.IsLocalUrl` trả về true.

## Logout và validation

Logout dùng POST, anti-forgery token và `SignOutAsync`. Login/Register đều dùng strongly typed ViewModel, ModelState validation, anti-forgery và thông báo đăng nhập thất bại chung.

## Database impact

Schema, entity, migration và seed data không thay đổi. S13 sử dụng cột `PasswordHash`, unique index và relationship User–Customer hiện có.

## Deferred

Admin authentication/UI, Role Management, Cart, Checkout, Order History, Forgot Password, email verification và OAuth chưa được triển khai.
