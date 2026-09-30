# S8 - Service Layer và Dependency Injection

## 1. Mục tiêu

S8 bổ sung Service Layer dùng chung để Web Controller và WinForms Form về sau không truy cập `SportEquipmentStoreDbContext` trực tiếp cho nghiệp vụ chính. Interfaces nằm trong Core, implementation nằm trong Data; không thêm project Application/Infrastructure hoặc Generic Repository.

```text
Web / Admin
     ↓
Service Layer
     ↓
EF Core DbContext
     ↓
SQL Server
```

Database `SportEquipmentStoreDb` đã được kiểm tra qua `tcp:localhost,1433`: server trả về `taiiiii\SERVER`, có 9 bảng nghiệp vụ, một migration `InitialCreate` và seed 2 Roles/6 Categories/12 Products. S8 không sửa entity, DbContext mapping, seed, schema hoặc migration.

## 2. Interfaces và implementations

| Interface trong Core | Implementation trong Data | Trách nhiệm |
|---|---|---|
| `ICategoryService` | `CategoryService` | Đọc, tạo, sửa và đổi trạng thái Category. |
| `IProductService` | `ProductService` | Đọc/search, tạo, sửa và đổi trạng thái Product. |
| `ICustomerService` | `CustomerService` | Quản lý hồ sơ Customer, không xử lý password/login. |
| `ICartService` | `CartService` | Đọc giỏ, thêm/cộng, cập nhật, xóa và làm rỗng giỏ. |
| `IOrderService` | `OrderService` | Đọc đơn, checkout từ giỏ, chuyển trạng thái và hủy đơn. |

`CreateOrderRequest` là input dùng chung trong Core, không phụ thuộc Web ViewModel. Nó chứa CustomerId, CheckoutRequestId, thông tin giao hàng và ghi chú; TotalAmount không nằm trong input vì service luôn tự tính.

Tất cả API truy cập dữ liệu dùng EF Core async và nhận `CancellationToken`. Query chỉ đọc dùng `AsNoTracking`; filter/search được thực hiện trong SQL trước `ToListAsync`. Không có `.Result`, `.Wait()` hoặc load toàn bộ rồi mới lọc.

## 3. Category business rules

`CategoryService` cung cấp `GetAllAsync`, `GetActiveAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `SetActiveAsync`. Tên bắt buộc, được trim và giới hạn 120 ký tự; Description tùy chọn, tối đa 1000 ký tự.

`SetActiveAsync(false)` chỉ thay đổi `Category.IsActive`, không tự đổi `Product.IsActive` và không hard delete. Admin dùng `GetAllAsync` để thấy cả active/inactive; Web dùng `GetActiveAsync`.

## 4. Product business rules

`ProductService` cung cấp các hàm yêu cầu cùng `GetActiveByCategoryAsync` để tách rõ truy vấn Web và Admin:

- `GetAllAsync` và `GetByCategoryAsync`: trả cả active/inactive cho nghiệp vụ quản trị.
- `GetActiveAsync`, `GetActiveByCategoryAsync`, `SearchAsync`: chỉ trả Product active thuộc Category active.
- `SearchAsync` trim keyword, tìm theo `ProductName` bằng biểu thức EF có thể dịch sang SQL; keyword null/rỗng trả danh sách active.
- Create/Update kiểm tra Category tồn tại, tên bắt buộc, `Price >= 0`, `StockQuantity >= 0` và giới hạn các chuỗi.
- Update sử dụng RowVersion đầu vào khi caller cung cấp để EF phát hiện ghi đè đồng thời.
- `SetActiveAsync` thay trạng thái, không hard delete.

Product vẫn có thể giữ `IsActive = true` khi Category inactive, nhưng không xuất hiện trong các query active dành cho Web.

## 5. Customer business rules

`CustomerService` cung cấp `GetAllAsync`, `GetByIdAsync`, `GetByUserIdAsync`, `CreateAsync`, `UpdateAsync`. Query kèm User/Role khi cần. Create yêu cầu User tồn tại, có Role `Customer` và chưa có hồ sơ Customer; Update chỉ sửa Phone/Address, không cho chuyển hồ sơ sang User khác. Service không đọc/sửa PasswordHash và không triển khai đăng nhập/đăng ký.

## 6. Cart business rules

`CartService` cung cấp `GetCartByCustomerAsync`, `AddItemAsync`, `UpdateQuantityAsync`, `RemoveItemAsync`, `ClearCartAsync`.

- Customer và Product phải tồn tại; Quantity phải dương.
- Product và Category phải active.
- Quantity không được vượt StockQuantity.
- Nếu Customer chưa có Cart, Add tạo Cart persistent khi cần.
- Nếu Product đã có trong Cart, Add cộng Quantity vào dòng hiện tại; tổng mới được kiểm tra tồn kho.
- Unique `(CartId, ProductId)` tiếp tục là lớp bảo vệ database.
- Add/Update chạy trong transaction `Serializable` khi chưa tham gia transaction ngoài; điều này bảo vệ chuỗi đọc-kiểm tra-ghi trước cập nhật đồng thời.
- Remove/Clear xóa CartItem tường minh, không xóa Cart hoặc Product.

Giỏ không giữ hàng và không chốt giá; OrderService luôn kiểm tra lại dữ liệu khi checkout.

## 7. Order business rules

`OrderService` cung cấp `GetAllAsync`, `GetByIdAsync`, `GetByCustomerAsync`, `CreateOrderFromCartAsync`, `UpdateStatusAsync` và `CancelByCustomerAsync`.

`CreateOrderFromCartAsync` thực hiện:

1. Validate CustomerId, CheckoutRequestId và thông tin giao hàng.
2. Trả lại đơn hiện có khi cùng `(CustomerId, CheckoutRequestId)` được retry.
3. Kiểm tra Customer, Cart và CartItem tồn tại; từ chối giỏ rỗng.
4. Kiểm tra Quantity dương, Product/Category active và tồn kho đủ.
5. Chụp `ProductNameAtPurchase` và `UnitPrice = Product.Price`.
6. Tự tính `TotalAmount = SUM(Quantity × UnitPrice)`; không nhận tổng từ UI.
7. Tạo Order/OrderDetails, trừ StockQuantity và xóa CartItems trong cùng transaction.

Trạng thái hợp lệ trong service:

```text
Pending ──→ Confirmed ──→ Shipping ──→ Completed
   │            │
   └────────────┴────────→ Cancelled
```

`UpdateStatusAsync` hỗ trợ luồng xử lý Admin/system và chỉ cho Cancelled từ Pending/Confirmed. `CancelByCustomerAsync` kiểm tra đúng chủ sở hữu và chỉ cho Customer hủy Pending. Authentication/authorization sẽ phải bảo đảm caller của `UpdateStatusAsync` là Admin ở bước sau.

## 8. Stock policy và hoàn tồn

Stock bị trừ khi Order được tạo thành công. Khi chuyển lần đầu sang Cancelled, StockQuantity được cộng lại từ OrderDetails. Lời gọi lặp lại với trạng thái Cancelled là no-op nên không hoàn lần hai.

Order và Product đều có RowVersion. Chuyển trạng thái, hoàn tồn và SaveChanges nằm trong một transaction; nếu có xung đột đồng thời hoặc lỗi ghi, toàn bộ transaction rollback. Cách này bảo đảm đúng một lần khi mọi cập nhật đi qua service. Việc sửa trực tiếp database không nằm trong bảo đảm này; nếu sau này cần audit/reconciliation xuyên hệ thống, nên thiết kế inventory ledger hoặc cờ phục hồi riêng trong một bước có migration được phê duyệt.

## 9. Transaction strategy

Cart Add/Update và Order Create/Status sử dụng `Serializable` transaction khi DbContext chưa có transaction. Nếu caller đã mở transaction, service tham gia transaction hiện có và không tự commit/rollback transaction của caller. Checkout bảo đảm nguyên tử cho Order, OrderDetails, stock và cart; không có trạng thái đơn tạo dở hoặc stock trừ dở.

RowVersion trên Product chống cập nhật tồn kho dựa trên dữ liệu cũ. RowVersion trên Order chống hai lần chuyển trạng thái cùng phiên bản. `CheckoutRequestId` cùng unique index bảo vệ idempotency checkout.

## 10. Exception và validation strategy

S8 không tạo Result framework phức tạp:

- `ArgumentException`/`ArgumentOutOfRangeException`: input thiếu, ID/Quantity sai, chuỗi quá dài, giá/tồn âm.
- `KeyNotFoundException`: entity bắt buộc không tồn tại.
- `InvalidOperationException`: inactive, thiếu tồn, giỏ rỗng, sai chủ sở hữu hoặc chuyển trạng thái không hợp lệ.
- `DbUpdateConcurrencyException`: xung đột RowVersion; tầng UI/API sau này cần hiển thị thông báo và yêu cầu tải lại.

Database constraints vẫn là lớp bảo vệ cuối, không thay thế validation service.

## 11. Dependency Injection

Web đăng ký cùng lifetime Scoped với DbContext:

```csharp
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
```

Admin đã reference Core và Data nên có thể đăng ký DbContext và năm mapping giống trên khi bước WinForms composition root được triển khai. S8 không thay đổi Form1 hoặc tạo Admin UI.

## 12. Kiểm tra

Đã chạy smoke test trên SQL Server thật trong một transaction ngoài và rollback toàn bộ. Các kiểm tra gồm:

- 6 Category active, 12 Product active và search có kết quả.
- Category inactive loại Product khỏi active query nhưng không đổi Product.IsActive; Cart từ chối Product thuộc Category inactive.
- Tạo/tra cứu Customer theo User.
- Add CartItem, add lại cùng Product, update Quantity, từ chối vượt stock, remove và clear.
- Từ chối checkout giỏ rỗng.
- Tạo Order; xác nhận UnitPrice snapshot, TotalAmount, giảm stock và xóa giỏ.
- Retry cùng CheckoutRequestId trả đúng Order cũ.
- Confirmed → Cancelled hoàn tồn; gọi Cancelled lần hai không hoàn lặp; chuyển trạng thái sai bị từ chối.
- Rollback verification xác nhận không còn User hoặc Order kiểm thử.

Build toàn solution và kiểm tra pending model phải tiếp tục PASS trước khi kết thúc S8.

## 13. Vấn đề chưa giải quyết

- Authentication/authorization chưa có; caller Admin của `UpdateStatusAsync` chưa được xác thực ở S8.
- Chính sách tên Category trùng và giá Product bằng 0 vẫn theo quyết định mở của S4.
- Chưa có inventory ledger/audit cho thay đổi stock ngoài Service Layer.
- UI cần xử lý concurrency exception, validation message và retry checkout an toàn.
- Tài liệu S7 ban đầu còn trạng thái kết nối cũ; S8 đã đồng bộ lại với kết quả TCP/database thực tế.
- `dotnet format --verify-no-changes` toàn solution phát hiện whitespace có sẵn trong `SportEquipmentStore.Admin/Program.cs`; S8 không sửa file Admin ngoài phạm vi. Kiểm tra format giới hạn trên toàn bộ file S8 đã PASS.

## 14. Để lại cho bước sau

Không triển khai Web/Admin UI, controller, authentication, login/register, Dashboard, CRUD WinForms, thống kê, báo cáo, thanh toán trực tuyến, review, wishlist hoặc promotion. S9 chưa bắt đầu.
