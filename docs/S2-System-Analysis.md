# S2 - Phân tích yêu cầu hệ thống SportEquipmentStore

## 1. Giới thiệu đề tài

SportEquipmentStore là hệ thống quản lý và bán dụng cụ thể thao, gồm hai ứng dụng sử dụng chung dữ liệu nghiệp vụ:

- Website ASP.NET Core MVC phục vụ khách hàng tra cứu và đặt mua sản phẩm.
- Ứng dụng Windows Forms hỗ trợ quản trị viên quản lý hoạt động bán hàng.

Tài liệu này xác định yêu cầu và phạm vi dự kiến của hệ thống. S2 chỉ phân tích, chưa khẳng định các chức năng đã được triển khai và không thiết kế cơ sở dữ liệu chi tiết.

## 2. Lý do xây dựng hệ thống

- Giúp khách hàng tìm kiếm, so sánh và đặt mua dụng cụ thể thao thuận tiện qua website.
- Tập trung dữ liệu danh mục, sản phẩm, khách hàng và đơn hàng thay cho quản lý thủ công, rời rạc.
- Hỗ trợ quản trị viên kiểm soát thông tin hàng hóa, tồn kho và quá trình xử lý đơn hàng.
- Tạo nền tảng có cấu trúc để áp dụng C#, ASP.NET Core MVC, Windows Forms, Entity Framework Core và SQL Server trong cùng một bài toán thực tế.

## 3. Mục tiêu

### 3.1. Mục tiêu nghiệp vụ

- Cung cấp quy trình mua hàng cơ bản từ xem sản phẩm đến đặt hàng và theo dõi đơn.
- Cung cấp công cụ quản trị dữ liệu bán hàng trên ứng dụng Desktop.
- Bảo đảm website và ứng dụng quản trị sử dụng nhất quán cùng một nguồn dữ liệu SQL Server.
- Hạn chế sai sót về giá, số lượng, tồn kho, tổng tiền và trạng thái đơn hàng bằng validation và quy tắc nghiệp vụ.

### 3.2. Mục tiêu kỹ thuật

- Sử dụng .NET 9 và C# với cấu trúc solution nhiều project hiện có.
- Tách giao diện, nghiệp vụ dùng chung và truy cập dữ liệu theo kiến trúc phân lớp.
- Thiết kế có khả năng bảo trì, kiểm tra lỗi và mở rộng mà không phá vỡ chức năng cốt lõi.
- Không lưu mật khẩu dạng rõ hoặc secret trong GitHub.

## 4. Phạm vi

### 4.1. Trong phạm vi phiên bản cốt lõi

- Website: trang chủ, danh mục, danh sách và chi tiết sản phẩm, tìm kiếm/lọc, đăng ký/đăng nhập, hồ sơ, giỏ hàng, thanh toán, đặt hàng và lịch sử đơn.
- Desktop Admin: đăng nhập, dashboard, quản lý danh mục, sản phẩm, khách hàng, đơn hàng, tài khoản/vai trò và thống kê cơ bản.
- Các ứng dụng dùng chung dữ liệu trên Microsoft SQL Server.
- Authentication, authorization, validation, xử lý lỗi và bảo đảm nhất quán dữ liệu ở mức phù hợp với đồ án.

### 4.2. Ngoài phạm vi bắt buộc

- Thanh toán trực tuyến qua cổng thanh toán thật.
- Kết nối đơn vị vận chuyển hoặc theo dõi vận đơn thời gian thực.
- Quản lý đa kho, đa chi nhánh hoặc chuỗi cửa hàng.
- Khuyến mãi phức tạp, tích điểm, đánh giá sản phẩm và gợi ý cá nhân hóa.
- Báo cáo nâng cao, xuất nhiều định dạng hoặc phân tích dự báo.
- Ứng dụng di động và Web API công khai.

Các nội dung trên chỉ là hướng mở rộng và không phải điều kiện hoàn thành phiên bản cốt lõi.

## 5. Đối tượng sử dụng (Actors)

### 5.1. Guest - Khách chưa đăng nhập

Guest có thể:

- Truy cập trang chủ.
- Xem danh mục sản phẩm.
- Xem danh sách và chi tiết sản phẩm đang hoạt động.
- Tìm kiếm và lọc sản phẩm.
- Đăng ký tài khoản.
- Đăng nhập.

Guest không được xem hồ sơ, lịch sử đơn hoặc đặt hàng dưới danh nghĩa khách hàng đã xác thực. Việc cho phép giỏ hàng tạm thời trước đăng nhập có thể cân nhắc sau; trước khi checkout phải xác định được khách hàng.

### 5.2. Customer - Khách hàng đã đăng nhập

Customer có toàn bộ quyền của Guest và có thể:

- Xem, cập nhật thông tin hồ sơ hợp lệ.
- Thêm sản phẩm đang hoạt động và còn hàng vào giỏ.
- Xem giỏ, thay đổi số lượng hoặc xóa CartItem.
- Thực hiện checkout với thông tin nhận hàng hợp lệ.
- Xác nhận đặt hàng.
- Xem lịch sử và chi tiết đơn của chính mình.
- Theo dõi trạng thái xử lý đơn.
- Yêu cầu hủy đơn khi trạng thái và chính sách cho phép.

Customer không được truy cập đơn hàng, hồ sơ hoặc dữ liệu riêng tư của khách hàng khác.

### 5.3. Admin - Quản trị viên

Admin phải đăng nhập và được phân quyền trước khi có thể:

- Xem dashboard.
- Quản lý danh mục.
- Quản lý sản phẩm và tồn kho.
- Xem, tìm kiếm và quản lý thông tin khách hàng trong phạm vi cần thiết.
- Xem và cập nhật trạng thái đơn hàng hợp lệ.
- Quản lý tài khoản và vai trò theo quyền được cấp.
- Tìm kiếm, lọc dữ liệu trên các màn hình quản lý.
- Xem thống kê cơ bản.
- Sử dụng báo cáo nâng cao ở giai đoạn mở rộng.

## 6. Yêu cầu chức năng

### 6.1. Web modules

#### Home

- Giới thiệu cửa hàng và điều hướng đến các nhóm sản phẩm chính.
- Hiển thị hero, danh mục nổi bật và sản phẩm nổi bật/mới nếu dữ liệu hỗ trợ.
- Cung cấp lối vào tìm kiếm, đăng nhập và giỏ hàng.

#### Products

- Hiển thị danh sách sản phẩm đang hoạt động.
- Hiển thị thông tin tóm tắt: tên, ảnh, giá, trạng thái còn hàng và danh mục.
- Hỗ trợ phân trang khi số lượng dữ liệu lớn.

#### Categories

- Hiển thị các danh mục đang hoạt động.
- Cho phép mở danh sách sản phẩm thuộc một danh mục.
- Không hiển thị danh mục bị ẩn cho Guest/Customer.

#### Product Detail

- Hiển thị thông tin đầy đủ của một sản phẩm: tên, mô tả, ảnh, giá, tồn kho và danh mục.
- Chỉ cho phép thêm vào giỏ khi sản phẩm đang hoạt động và số lượng hợp lệ.
- Thông báo rõ khi sản phẩm hết hàng hoặc ngừng bán.

#### Search/Filter

- Tìm kiếm theo từ khóa phù hợp với tên hoặc thông tin sản phẩm.
- Lọc tối thiểu theo danh mục; lọc theo khoảng giá hoặc tình trạng hàng là SHOULD.
- Kết quả phải phản ánh dữ liệu hiện hành và không làm lộ sản phẩm bị ẩn.

#### Register/Login

- Đăng ký bằng thông tin định danh duy nhất theo thiết kế (username/email).
- Kiểm tra định dạng, dữ liệu bắt buộc và xác nhận mật khẩu.
- Đăng nhập bằng thông tin hợp lệ; từ chối tài khoản inactive.
- Hiển thị lỗi thân thiện nhưng không tiết lộ chi tiết nhạy cảm.

#### Profile

- Hiển thị thông tin của Customer đang đăng nhập.
- Cho phép cập nhật các trường được phép như họ tên, số điện thoại và địa chỉ.
- Không cho phép chỉnh sửa trực tiếp vai trò hoặc dữ liệu bảo mật không phù hợp.

#### Cart

- Thêm sản phẩm, xem các CartItem, cập nhật số lượng và xóa sản phẩm.
- Kiểm tra sản phẩm, giá tham khảo và tồn kho khi thêm/cập nhật.
- Tính tạm tính theo số lượng và giá hiện tại để người dùng tham khảo.
- Kiểm tra lại toàn bộ giỏ tại checkout vì dữ liệu có thể thay đổi.

#### Checkout

- Yêu cầu Customer đăng nhập.
- Thu thập/xác nhận thông tin nhận hàng cần thiết.
- Kiểm tra giỏ không rỗng, sản phẩm còn hoạt động, số lượng hợp lệ và không vượt tồn kho.
- Hiển thị bản tóm tắt đơn và tổng tiền trước khi xác nhận.

#### Orders

- Tạo đơn và ít nhất một chi tiết đơn trong một thao tác nhất quán.
- Lưu giá tại thời điểm mua vào OrderDetail.
- Hiển thị lịch sử, chi tiết và trạng thái đơn cho đúng chủ sở hữu.
- Cho phép hủy đơn chỉ khi quy tắc nghiệp vụ cho phép.

### 6.2. Admin modules

Mọi màn hình quản lý cần dự kiến hỗ trợ xem dữ liệu, tìm kiếm/lọc, validation và xử lý lỗi. Thêm, sửa, xóa hoặc ẩn được áp dụng tùy tính chất dữ liệu; không xóa cứng dữ liệu đã tham gia giao dịch nếu làm mất lịch sử.

#### Login

- Xác thực tài khoản quản trị đang hoạt động.
- Kiểm tra vai trò/quyền trước khi mở màn hình quản trị.
- Thông báo thất bại an toàn, không tiết lộ mật khẩu hoặc trạng thái nội bộ không cần thiết.

#### Dashboard

- Hiển thị thông tin tổng quan như số sản phẩm, khách hàng, đơn hàng và số đơn theo trạng thái.
- Có thể hiển thị doanh thu hoặc sản phẩm sắp hết hàng khi dữ liệu và phạm vi cho phép.
- Dashboard chỉ phục vụ tổng quan, không thay thế màn hình quản lý chi tiết.

#### Category Management

- Xem, thêm, sửa và tìm kiếm/lọc danh mục.
- Ưu tiên ẩn/ngừng hoạt động thay vì xóa khi danh mục đã có sản phẩm liên quan.
- Kiểm tra tên bắt buộc và quy tắc trùng tên nếu được chốt trong thiết kế.

#### Product Management

- Xem, thêm, sửa, tìm kiếm và lọc sản phẩm.
- Quản lý giá, tồn kho, danh mục, mô tả, ảnh và trạng thái hoạt động.
- Không cho phép giá hoặc tồn kho âm.
- Ưu tiên ngừng bán/ẩn thay vì xóa sản phẩm đã xuất hiện trong đơn hàng.

#### Customer Management

- Xem danh sách và thông tin cần thiết của khách hàng.
- Tìm kiếm/lọc theo thông tin phù hợp và trạng thái tài khoản.
- Cho phép cập nhật hoặc khóa/mở tài khoản theo quyền; không hiển thị mật khẩu.
- Việc xóa cứng khách hàng đã có đơn hàng không được phép.

#### Order Management

- Xem danh sách và chi tiết đơn, tìm kiếm/lọc theo mã đơn, khách hàng, ngày hoặc trạng thái.
- Cập nhật trạng thái theo chuỗi chuyển trạng thái hợp lệ.
- Không sửa tùy ý UnitPrice đã chốt hoặc chi tiết làm sai lịch sử giao dịch.
- Xử lý hủy đơn và ảnh hưởng tồn kho theo chính sách được chốt ở bước thiết kế.

#### Account/Role Management

- Xem, thêm hoặc cập nhật tài khoản quản trị theo quyền.
- Gán vai trò/quyền phù hợp; vô hiệu hóa tài khoản thay vì xóa khi cần bảo toàn dấu vết.
- Không hiển thị hoặc lưu mật khẩu dạng rõ.
- Ngăn thao tác khiến hệ thống không còn tài khoản quản trị hợp lệ nếu quy mô đồ án yêu cầu.

#### Statistics/Reports

- MUST: thống kê cơ bản phục vụ dashboard, ví dụ số đơn theo trạng thái và tổng quan doanh thu từ đơn hợp lệ.
- SHOULD: lọc thống kê theo khoảng thời gian.
- NICE TO HAVE: báo cáo chi tiết, biểu đồ nâng cao, xuất Excel/PDF và thống kê sản phẩm bán chạy.

## 7. Phân loại chức năng

| Feature | Actor | Priority | Description |
|---|---|---|---|
| Trang chủ | Guest, Customer | MUST | Hiển thị thông tin và điều hướng mua sắm chính. |
| Xem danh mục | Guest, Customer | MUST | Xem các danh mục đang hoạt động. |
| Xem danh sách sản phẩm | Guest, Customer | MUST | Duyệt các sản phẩm đang hoạt động. |
| Xem chi tiết sản phẩm | Guest, Customer | MUST | Xem đầy đủ thông tin, giá và tình trạng hàng. |
| Tìm kiếm sản phẩm | Guest, Customer | MUST | Tìm theo từ khóa. |
| Lọc theo danh mục | Guest, Customer | MUST | Thu hẹp danh sách theo danh mục. |
| Lọc theo giá/tình trạng hàng | Guest, Customer | SHOULD | Hỗ trợ tìm sản phẩm phù hợp nhanh hơn. |
| Đăng ký | Guest | MUST | Tạo tài khoản khách hàng hợp lệ. |
| Đăng nhập/đăng xuất Web | Guest, Customer | MUST | Xác thực phiên sử dụng website. |
| Quản lý hồ sơ | Customer | MUST | Xem và cập nhật thông tin cá nhân được phép. |
| Thêm vào giỏ | Customer | MUST | Thêm sản phẩm hợp lệ với số lượng hợp lệ. |
| Xem/cập nhật/xóa CartItem | Customer | MUST | Quản lý các mặt hàng trước khi checkout. |
| Checkout và đặt hàng | Customer | MUST | Kiểm tra giỏ, thông tin nhận hàng và tạo đơn. |
| Lịch sử đơn | Customer | MUST | Xem các đơn thuộc tài khoản hiện tại. |
| Chi tiết đơn | Customer | MUST | Xem sản phẩm, giá chốt, tổng tiền và trạng thái. |
| Theo dõi trạng thái đơn | Customer | MUST | Theo dõi tiến trình xử lý đơn. |
| Yêu cầu hủy đơn | Customer | SHOULD | Hủy khi trạng thái và chính sách cho phép. |
| Đăng nhập Admin | Admin | MUST | Xác thực và phân quyền ứng dụng Desktop. |
| Dashboard | Admin | MUST | Xem tổng quan dữ liệu vận hành. |
| Quản lý danh mục | Admin | MUST | Xem, thêm, sửa, ẩn và tìm kiếm danh mục. |
| Quản lý sản phẩm | Admin | MUST | Xem, thêm, sửa, ẩn, tìm kiếm và quản lý tồn kho. |
| Quản lý khách hàng | Admin | MUST | Xem, tìm kiếm và quản lý trạng thái phù hợp. |
| Quản lý đơn hàng | Admin | MUST | Xem, lọc và cập nhật trạng thái hợp lệ. |
| Quản lý tài khoản/vai trò | Admin | MUST | Quản lý tài khoản và quyền truy cập quản trị. |
| Thống kê cơ bản | Admin | MUST | Tổng hợp số liệu cốt lõi cho dashboard. |
| Phân trang danh sách lớn | Guest, Customer, Admin | SHOULD | Cải thiện tốc độ và khả năng sử dụng. |
| Báo cáo nâng cao | Admin | NICE TO HAVE | Phân tích chi tiết theo thời gian/sản phẩm. |
| Xuất Excel/PDF | Admin | NICE TO HAVE | Xuất dữ liệu hoặc báo cáo. |
| Thanh toán trực tuyến | Customer | NICE TO HAVE | Tích hợp cổng thanh toán ở giai đoạn mở rộng. |
| Theo dõi vận chuyển thời gian thực | Customer, Admin | NICE TO HAVE | Tích hợp đơn vị vận chuyển ngoài hệ thống. |
| Khuyến mãi/điểm thưởng | Customer, Admin | NICE TO HAVE | Hỗ trợ chương trình giữ chân khách hàng. |
| Đánh giá sản phẩm | Customer | NICE TO HAVE | Gửi đánh giá sau khi mua hàng hợp lệ. |

## 8. Yêu cầu phi chức năng

### 8.1. Responsive Web

- Giao diện Web sử dụng được trên desktop, tablet và điện thoại ở các kích thước phổ biến.
- Nội dung, menu, form, danh sách sản phẩm và giỏ hàng không bị tràn hoặc mất thao tác chính.

### 8.2. Usability

- Điều hướng nhất quán, nhãn và thông báo rõ ràng bằng ngôn ngữ phù hợp với người dùng.
- Thao tác quan trọng như đặt hàng, hủy đơn, xóa/ẩn phải có xác nhận khi cần.
- Trạng thái tải, rỗng, thành công và thất bại cần được biểu diễn dễ hiểu.

### 8.3. Security

- Mọi chức năng riêng tư hoặc quản trị phải kiểm tra authentication và authorization ở phía hệ thống, không chỉ ẩn nút trên UI.
- Chống các rủi ro phổ biến phù hợp nền tảng như giả mạo yêu cầu, truy cập trái phép và dữ liệu đầu vào độc hại.
- Không hiển thị thông tin lỗi nội bộ hoặc dữ liệu nhạy cảm cho người dùng cuối.
- Áp dụng nguyên tắc quyền tối thiểu cho tài khoản và vai trò.

### 8.4. Password hashing

- Mật khẩu không được lưu dạng rõ hoặc mã hóa có thể đảo ngược.
- Sử dụng cơ chế băm mật khẩu có salt và thư viện chuẩn, thay vì tự xây dựng thuật toán.
- Không ghi mật khẩu vào log, thông báo lỗi hoặc mã nguồn.

### 8.5. Validation

- Kiểm tra dữ liệu ở UI để hỗ trợ trải nghiệm và kiểm tra lại tại lớp xử lý nghiệp vụ.
- Thông báo validation cần chỉ rõ trường hoặc quy tắc không hợp lệ.
- Không tin cậy dữ liệu gửi từ client dù đã có client-side validation.

### 8.6. Exception handling

- Bắt và xử lý ngoại lệ ở ranh giới phù hợp, không bỏ qua lỗi im lặng.
- Ghi log đủ thông tin kỹ thuật để chẩn đoán nhưng không chứa secret hoặc mật khẩu.
- Hiển thị thông báo thân thiện và bảo toàn trạng thái dữ liệu khi thao tác thất bại.

### 8.7. Maintainability và layered architecture

- Giữ vai trò hiện tại: Web/Admin là UI, Core chứa domain/thành phần dùng chung, Data phụ trách truy cập dữ liệu.
- Tránh đặt truy vấn dữ liệu hoặc quy tắc nghiệp vụ phức tạp trực tiếp trong UI.
- Không tạo circular dependency; ưu tiên thành phần nhỏ, tên rõ ràng và trách nhiệm đơn nhất.

### 8.8. Data consistency

- Các thao tác nhiều bước như tạo Order, OrderDetails và cập nhật tồn kho phải nhất quán; khi một bước thất bại không được để dữ liệu dở dang.
- Tổng tiền và trạng thái phải được kiểm soát ở phía hệ thống.
- Cần có chiến lược xử lý xung đột tồn kho khi nhiều người mua cùng lúc ở bước thiết kế/triển khai sau.

### 8.9. Quản lý cấu hình và secret

- Không commit mật khẩu, connection string nhạy cảm, token hoặc secret lên GitHub.
- Cấu hình local nhạy cảm sử dụng User Secrets, biến môi trường hoặc file local đã được `.gitignore` loại trừ.
- Cấu hình mẫu nếu có chỉ chứa giá trị giả, không chứa thông tin truy cập thật.

## 9. Quy tắc nghiệp vụ

| Mã | Quy tắc | Ý nghĩa/kiểm soát dự kiến |
|---|---|---|
| BR01 | `Price >= 0`. | Không chấp nhận giá âm khi thêm/sửa sản phẩm. Giá bằng 0 chỉ dùng nếu nghiệp vụ cho phép. |
| BR02 | `StockQuantity >= 0`. | Tồn kho không được âm sau bất kỳ thao tác hợp lệ nào. |
| BR03 | Số lượng đặt mua phải lớn hơn 0. | CartItem/OrderDetail có `Quantity > 0`. |
| BR04 | Không được đặt vượt tồn kho. | Kiểm tra khi cập nhật giỏ và bắt buộc kiểm tra lại khi tạo đơn. |
| BR05 | Order phải có ít nhất một OrderDetail. | Không tạo đơn rỗng. |
| BR06 | OrderDetail lưu UnitPrice tại thời điểm mua. | Giá lịch sử không thay đổi khi giá Product thay đổi sau đó. |
| BR07 | TotalAmount được tính từ OrderDetails. | `TotalAmount = tổng(Quantity × UnitPrice)`; không tin giá trị tổng gửi từ client. |
| BR08 | Username/Email cần unique theo thiết kế. | Chuẩn hóa cách so sánh và dùng ràng buộc dữ liệu để tránh trùng. Cần chốt dùng cả hai hay email làm định danh chính. |
| BR09 | Admin phải được authentication và authorization. | Đăng nhập thành công chưa đủ; chức năng cần kiểm tra vai trò/quyền. |
| BR10 | Account inactive không được đăng nhập. | Phiên hiện có cũng cần chính sách xử lý khi tài khoản bị vô hiệu hóa. |
| BR11 | Product inactive không được mua mới. | Có thể giữ trong lịch sử đơn nhưng không thêm mới/checkout. |
| BR12 | Web và Desktop dùng chung dữ liệu SQL Server. | Thay đổi hợp lệ từ một ứng dụng phải được ứng dụng kia nhìn thấy theo dữ liệu hiện hành. |
| BR13 | Trạng thái đơn chuyển theo luồng hợp lệ. | Luồng chuẩn: `Pending -> Confirmed -> Shipping -> Completed`; `Cancelled` chỉ khi nghiệp vụ cho phép. |
| BR14 | Customer chỉ xem được dữ liệu của chính mình. | Kiểm tra quyền sở hữu hồ sơ, giỏ và đơn ở phía server. |
| BR15 | Dữ liệu giao dịch cần được bảo toàn. | Sản phẩm, khách hàng hoặc danh mục đã được tham chiếu nên ẩn/inactive thay vì xóa cứng. |
| BR16 | Mật khẩu không lưu dạng rõ. | Chỉ lưu password hash theo cơ chế an toàn. |

### 9.1. Ma trận chuyển trạng thái đơn dự kiến

| Trạng thái hiện tại | Trạng thái kế tiếp hợp lệ | Ghi chú |
|---|---|---|
| Pending | Confirmed, Cancelled | Customer/Admin có thể hủy tùy chính sách; Admin xác nhận đơn. |
| Confirmed | Shipping, Cancelled | Hủy sau xác nhận chỉ khi chưa giao và nghiệp vụ cho phép. |
| Shipping | Completed | Thông thường không cho hủy khi đang giao; ngoại lệ cần quy trình riêng nếu mở rộng. |
| Completed | Không có | Trạng thái kết thúc. |
| Cancelled | Không có | Trạng thái kết thúc; cần chốt cách hoàn tồn kho. |

Không được bỏ qua tùy ý một trạng thái hoặc chuyển ngược nếu chưa có nghiệp vụ được phê duyệt.

## 10. Luồng nghiệp vụ chính

### 10.1. Khám phá sản phẩm

1. Guest/Customer mở trang chủ.
2. Người dùng chọn danh mục hoặc nhập từ khóa tìm kiếm.
3. Hệ thống trả danh sách sản phẩm đang hoạt động phù hợp bộ lọc.
4. Người dùng mở chi tiết sản phẩm để xem giá, mô tả và tồn kho.

### 10.2. Đăng ký và đăng nhập

1. Guest nhập thông tin đăng ký.
2. Hệ thống validation và kiểm tra username/email không trùng.
3. Mật khẩu được băm trước khi lưu ở giai đoạn triển khai.
4. Người dùng đăng nhập; hệ thống kiểm tra thông tin xác thực và trạng thái active.
5. Nếu hợp lệ, hệ thống thiết lập phiên và quyền Customer.

### 10.3. Giỏ hàng và đặt hàng

1. Customer chọn sản phẩm đang hoạt động và số lượng lớn hơn 0.
2. Hệ thống kiểm tra tồn kho rồi thêm/cập nhật CartItem.
3. Customer xem giỏ, thay đổi số lượng hoặc xóa mặt hàng.
4. Customer chuyển đến checkout và xác nhận thông tin nhận hàng.
5. Hệ thống kiểm tra lại toàn bộ giỏ, trạng thái sản phẩm, giá và tồn kho.
6. Hệ thống tạo Order cùng OrderDetails, chốt UnitPrice, tính TotalAmount và cập nhật tồn kho trong một giao dịch nhất quán.
7. Đơn được tạo ở trạng thái Pending; giỏ được xử lý theo chính sách sau khi tạo đơn thành công.
8. Customer nhận kết quả và có thể xem đơn trong lịch sử.

### 10.4. Xử lý đơn hàng

1. Admin đăng nhập và mở danh sách đơn.
2. Admin tìm/lọc đơn Pending và kiểm tra chi tiết.
3. Admin xác nhận đơn: Pending chuyển thành Confirmed.
4. Khi bàn giao vận chuyển: Confirmed chuyển thành Shipping.
5. Khi hoàn tất: Shipping chuyển thành Completed.
6. Nếu hủy hợp lệ, đơn chuyển thành Cancelled và hệ thống xử lý tồn kho theo chính sách đã chốt.
7. Customer theo dõi trạng thái cập nhật trên website.

### 10.5. Quản lý sản phẩm

1. Admin mở Product Management và tìm/lọc dữ liệu.
2. Admin thêm hoặc sửa thông tin sản phẩm.
3. Hệ thống validation giá, tồn kho, danh mục và các trường bắt buộc.
4. Nếu hợp lệ, thay đổi được lưu; nếu không, UI hiển thị lỗi có thể sửa.
5. Sản phẩm không còn kinh doanh được đặt inactive thay vì xóa khi đã có lịch sử giao dịch.

## 11. Định hướng UI/UX

### 11.1. Website

- Phong cách thương mại điện tử thể thao hiện đại, khỏe khoắn, rõ thứ bậc thông tin.
- Header nhất quán gồm logo/tên hệ thống, điều hướng, ô tìm kiếm, tài khoản và giỏ hàng.
- Hero trên trang chủ truyền tải chủ đề thể thao và dẫn đến hành động mua sắm chính.
- Category card và product card nhất quán về ảnh, tên, giá, tồn kho và nút hành động.
- Thiết kế responsive; ưu tiên thao tác chạm và nội dung quan trọng trên màn hình nhỏ.
- Form có nhãn, validation gần trường nhập và trạng thái focus rõ ràng.
- Dùng màu, typography, khoảng cách, icon và thông báo nhất quán; bảo đảm độ tương phản phù hợp.

### 11.2. Admin Desktop

- Màn hình Login đơn giản, rõ thông báo và không để lộ thông tin nhạy cảm.
- Bố cục chính dùng sidebar điều hướng giữa Dashboard và các module quản lý.
- Dashboard hiển thị thẻ số liệu và thống kê dễ đọc, không quá tải thông tin.
- Danh sách quản lý sử dụng DataGridView, có tìm kiếm/lọc, chọn bản ghi và trạng thái rỗng/lỗi.
- Form CRUD tách trường hợp thêm/sửa rõ ràng, validation trước khi lưu và xác nhận thao tác nguy hiểm.
- Các nút, màu trạng thái, hộp thoại và cách đặt tên thống nhất giữa các module.

## 12. Chức năng mở rộng

Các chức năng sau là NICE TO HAVE, chỉ xem xét sau khi phần MUST ổn định:

- Báo cáo nâng cao, biểu đồ theo thời gian, xuất Excel/PDF.
- Thanh toán trực tuyến.
- Kết nối vận chuyển và theo dõi vận đơn.
- Mã giảm giá, chương trình khuyến mãi và tích điểm.
- Danh sách yêu thích, đánh giá và nhận xét sản phẩm.
- Cảnh báo tồn kho thấp và hỗ trợ nhập kho chi tiết.
- Email/thông báo thay đổi trạng thái đơn.
- Gợi ý sản phẩm và thống kê hành vi.
- Hỗ trợ nhiều ảnh sản phẩm, đa kho hoặc đa chi nhánh.

Các chức năng mở rộng không được biến thành điều kiện bắt buộc của phiên bản đồ án cốt lõi.

## 13. Đối chiếu yêu cầu môn học

| Nội dung học tập/kỹ thuật | Định hướng đáp ứng của hệ thống | Phạm vi dự kiến |
|---|---|---|
| Lập trình C# và .NET 9 | Toàn bộ solution sử dụng C#/.NET 9. | MUST |
| Lập trình hướng đối tượng | Domain/model và nghiệp vụ dùng chung được tổ chức trong Core. | MUST |
| ASP.NET Core MVC | Website khách hàng theo mô hình MVC. | MUST |
| Windows Forms | Ứng dụng Desktop dành cho Admin. | MUST |
| Cơ sở dữ liệu quan hệ | Dữ liệu nghiệp vụ dùng chung trên SQL Server. | MUST |
| Truy cập dữ liệu | Dự kiến sử dụng Entity Framework Core tại tầng Data. | MUST |
| Validation và xử lý lỗi | Kiểm tra dữ liệu, xử lý ngoại lệ và phản hồi thân thiện. | MUST |
| Authentication/Authorization | Phân biệt Customer/Admin và bảo vệ chức năng riêng tư. | MUST |
| Kiến trúc phân lớp | Web/Admin phụ thuộc Core/Data; Data phụ thuộc Core; không phụ thuộc vòng. | MUST |
| Quản lý mã nguồn | Git/GitHub, `.gitignore`, không commit build artifacts hoặc secret. | MUST |
| Thống kê/báo cáo | Thống kê cơ bản là MUST; báo cáo nâng cao là mở rộng. | MUST/NICE TO HAVE |

Việc đối chiếu trên dựa vào phạm vi đề tài đã cung cấp. Nếu đề cương môn học có rubric chi tiết hơn (số lượng màn hình, loại báo cáo, yêu cầu demo hoặc tài liệu), cần đối chiếu thêm với rubric chính thức trước khi nghiệm thu.

## 14. Rủi ro và vấn đề cần chốt trước các bước thiết kế/triển khai

- Cần chốt trường đăng nhập chính: username, email hoặc hỗ trợ cả hai; đồng thời xác định quy tắc không phân biệt hoa/thường.
- Cần chốt chính sách giỏ hàng của Guest và cách xử lý khi Guest đăng nhập.
- Cần chốt các trường bắt buộc của địa chỉ nhận hàng và phương thức thanh toán trong phạm vi đồ án.
- Cần chốt thời điểm trừ/hoàn tồn kho, đặc biệt khi đơn bị hủy hoặc nhiều checkout đồng thời.
- Cần chốt ai được hủy đơn và hủy ở trạng thái nào.
- Cần chốt cách tính doanh thu: chỉ đơn Completed hay gồm trạng thái khác; cách xử lý đơn Cancelled.
- Cần chốt mô hình vai trò đơn giản (Customer/Admin) hay quyền chi tiết cho nhiều loại nhân viên.
- Cần chốt chính sách xóa mềm/inactive và dữ liệu nào tuyệt đối không được xóa cứng.
- Ảnh sản phẩm cần quyết định lưu đường dẫn/tệp hay dịch vụ ngoài; không nên lưu tùy tiện dữ liệu ảnh lớn trong thiết kế ban đầu.
- Web và Admin cùng cập nhật dữ liệu có nguy cơ xung đột; bước thiết kế cần xem xét transaction và concurrency.
- Báo cáo nâng cao, thanh toán và vận chuyển phải giữ ở phạm vi mở rộng để tránh làm trễ chức năng MUST.

## 15. Giới hạn của S2

S2 không tạo hoặc triển khai:

- Entity, DbContext hoặc cấu hình Entity Framework Core.
- Migration, SQL script hoặc SQL Server database.
- Controller, repository, service hoặc chức năng CRUD.
- Authentication/authorization thực tế.
- Cart, checkout hoặc xử lý đơn thực tế.
- Giao diện Web hoặc Windows Forms mới.

Tài liệu này là đầu vào phân tích cho các bước sau và không phải bằng chứng rằng các chức năng đã hoàn thành.
