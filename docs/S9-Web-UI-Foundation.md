# S9 - Customer Web Layout & UI Foundation

## 1. Mục tiêu

S9 xây dựng nền tảng giao diện dùng chung cho website khách hàng của Sport Equipment Store. Phạm vi gồm layout, header, điều hướng, giao diện tìm kiếm/tài khoản/giỏ hàng, footer, hệ thống thiết kế, responsive và các partial có thể tái sử dụng ở S10-S16.

S9 không triển khai nghiệp vụ tìm kiếm, xác thực, giỏ hàng, checkout, catalog sản phẩm hoặc giao diện Admin.

## 2. Cấu trúc layout

`Views/Shared/_Layout.cshtml` tổ chức trang theo thứ tự:

1. Skip link cho người dùng bàn phím.
2. Top bar với thông tin vận chuyển và hỗ trợ.
3. Main header gồm brand, search UI, account UI và cart UI.
4. Main navigation cho desktop và menu collapse cho tablet/mobile.
5. Vùng nội dung chính qua `RenderBody`.
6. Trust/benefit section dùng chung.
7. Footer bốn nhóm nội dung.
8. Bootstrap bundle và JavaScript dùng chung.

Các chức năng chưa được triển khai dùng trạng thái disabled hoặc liên kết an toàn; không tạo link tới route chưa tồn tại.

## 3. Header

### Top bar

- Thông báo miễn phí vận chuyển với đơn đủ điều kiện.
- Nhãn hỗ trợ khách hàng và đổi trả thuận tiện.
- Trên mobile chỉ giữ thông điệp chính để tránh chật giao diện.

### Main header

- Brand sử dụng logo SVG nội bộ và tên `SPORT EQUIPMENT STORE`.
- Search bar có label ẩn, icon, input và button; button đang disabled vì search backend thuộc S12.
- Account là UI disabled, không có user giả và không có authentication.
- Cart là UI disabled, badge luôn là `0`, không gọi `ICartService` khi chưa có customer identity.

## 4. Navigation

Navigation gồm:

- Trang chủ: liên kết tới `Home/Index`.
- Sản phẩm: disabled, chờ catalog ở S11.
- Danh mục: dropdown được render bằng `CategoryNavigationViewComponent`.
- Giới thiệu và Liên hệ: liên kết tới section an toàn trên trang chủ.

`CategoryNavigationViewComponent` gọi `ICategoryService.GetActiveAsync`. Razor View không inject `DbContext` và không chứa EF query. Tên category chỉ có một nguồn là dữ liệu hiện tại trong SQL Server. Các item category chưa điều hướng vì trang category thuộc bước sau.

Bootstrap bundle xử lý dropdown và navbar collapse; không thêm framework JavaScript mới.

## 5. Footer

Footer có bốn nhóm:

1. Brand và mô tả ngắn.
2. Liên kết nhanh.
3. Hỗ trợ khách hàng.
4. Thông tin liên hệ minh họa.

Thông tin email, hotline và địa chỉ đều là dữ liệu demo, không dùng thông tin cá nhân thật. Footer hiển thị copyright `© 2026 Sport Equipment Store`.

## 6. Responsive strategy

- Desktop: header ba cột; navigation hiển thị đầy đủ; benefit và footer bốn cột.
- Tablet: search chuyển xuống hàng riêng; account/cart giữ icon; navigation collapse; benefit và footer hai cột.
- Mobile: header gọn, search vẫn đủ input/button, navigation collapse, home intro một cột, benefit/footer xếp một cột.
- Grid dùng `minmax(0, 1fr)` và các breakpoint tương thích Bootstrap nhằm tránh horizontal overflow.
- Typography và padding dùng `clamp()` tại các vùng chính để co giãn tự nhiên.
- Có hỗ trợ `prefers-reduced-motion`.

Kết quả đo bằng Chrome DevTools emulation:

| Viewport | Client width | Scroll width | Horizontal overflow | Menu |
|---:|---:|---:|---|---|
| 1440 px | 1440 px | 1440 px | Không | Desktop navigation và category dropdown hoạt động |
| 768 px | 768 px | 768 px | Không | Collapse mở đúng, `aria-expanded=true` |
| 375 px | 375 px | 375 px | Không | Collapse mở đúng, `aria-expanded=true` |

## 7. Design system

Design tokens được đặt trong `wwwroot/css/site.css`:

- Màu: navy, primary blue, surface trắng, nền xám nhạt, text/muted/border và accent cam dùng tiết chế.
- Typography: system font stack ưu tiên Segoe UI, có cấp độ rõ cho brand, heading, body và helper text.
- Radius: ba mức `sm`, `md`, `lg`.
- Shadow: hai mức `sm`, `md`.
- Control: chiều cao chuẩn cho input/button header.
- Transition: một token dùng chung và tự giảm khi người dùng bật reduced motion.
- Container: giới hạn `76rem`, gutter co giãn theo breakpoint.

Không dùng inline style và không dùng `!important`.

## 8. Icon strategy

Icon được lưu trong SVG sprite nội bộ `wwwroot/images/icons/ui-icons.svg`, gồm:

- search
- person/account
- cart
- truck/shipping
- shield-check
- return
- headset/support

Partial `_Icon.cshtml` tạo markup dùng lại. Giải pháp này không phụ thuộc CDN, không hotlink và không cài thêm icon library.

## 9. Image strategy

- Logo: `wwwroot/images/branding/logo-mark.svg`.
- Placeholder sản phẩm: `wwwroot/images/placeholders/product-placeholder.svg`.
- Tài nguyên đều nằm trong project và được ASP.NET Core Static Web Assets phục vụ.
- `_ProductCard.cshtml` tự dùng placeholder nội bộ khi `ImageUrl` rỗng.
- S9 không thêm ảnh sản phẩm giả hoặc URL ảnh bên ngoài.

## 10. Reusable components

- `_Icon.cshtml`: render một icon từ SVG sprite.
- `_TrustBenefits.cshtml`: bốn lợi ích dùng chung trước footer.
- `_ProductCard.cshtml`: nền tảng card gồm ảnh, category, tên, giá, tồn kho và nút chi tiết.
- `ProductCardViewModel`: model rõ ràng dành riêng cho product card.
- `CategoryNavigationViewComponent`: lấy category active qua service layer.
- `Views/Shared/Components/CategoryNavigation/Default.cshtml`: giao diện dropdown category.

Product card đã được chuẩn bị nhưng chưa render danh sách sản phẩm ở Home; dữ liệu catalog thuộc S10/S11.

## 11. Home placeholder

`Views/Home/Index.cshtml` chỉ có intro nhẹ để đánh giá layout và responsive. Nội dung không truy vấn product, không hard-code danh sách seed, không xây featured categories/products và không phải homepage thương mại hoàn chỉnh.

## 12. Files created

- `docs/S9-Web-UI-Foundation.md`
- `src/SportEquipmentStore.Web/Models/ProductCardViewModel.cs`
- `src/SportEquipmentStore.Web/ViewComponents/CategoryNavigationViewComponent.cs`
- `src/SportEquipmentStore.Web/Views/Shared/_Icon.cshtml`
- `src/SportEquipmentStore.Web/Views/Shared/_ProductCard.cshtml`
- `src/SportEquipmentStore.Web/Views/Shared/_TrustBenefits.cshtml`
- `src/SportEquipmentStore.Web/Views/Shared/Components/CategoryNavigation/Default.cshtml`
- `src/SportEquipmentStore.Web/wwwroot/images/branding/logo-mark.svg`
- `src/SportEquipmentStore.Web/wwwroot/images/icons/ui-icons.svg`
- `src/SportEquipmentStore.Web/wwwroot/images/placeholders/product-placeholder.svg`

## 13. Files modified

- `src/SportEquipmentStore.Web/Views/Shared/_Layout.cshtml`
- `src/SportEquipmentStore.Web/Views/Shared/_Layout.cshtml.css`
- `src/SportEquipmentStore.Web/Views/Home/Index.cshtml`
- `src/SportEquipmentStore.Web/Views/Home/Privacy.cshtml`
- `src/SportEquipmentStore.Web/wwwroot/css/site.css`

## 14. Manual test instructions

Từ thư mục repository:

```powershell
dotnet restore SportEquipmentStore.sln
dotnet build SportEquipmentStore.sln --no-restore
dotnet run --project src/SportEquipmentStore.Web
```

Mở URL do terminal cung cấp và kiểm tra:

1. Home trả về thành công; header, intro, benefit và footer xuất hiện.
2. Logo, CSS, Bootstrap JavaScript và icon SVG tải thành công.
3. Dropdown Danh mục hiển thị 6 category active từ database.
4. Search, Account, Cart và các chức năng chưa có không điều hướng tới trang 404.
5. Thu nhỏ viewport về 1440, 768 và 375 px; xác nhận không có thanh cuộn ngang.
6. Ở 768/375 px, dùng bàn phím hoặc click button Menu; xác nhận navigation mở/đóng và `aria-expanded` đổi trạng thái.
7. Dùng phím Tab để kiểm tra skip link, focus outline, navigation và các link hoạt động.

## 15. Accessibility foundation

- `html` dùng ngôn ngữ `vi`.
- Có skip link tới vùng `main`.
- Header, nav, main và footer dùng semantic landmarks.
- Input tìm kiếm có label và phần mô tả trạng thái.
- Button menu có `aria-controls`, `aria-expanded` và accessible label.
- Icon trang trí dùng `aria-hidden`; logo trang trí có alt rỗng vì brand text đã hiện cạnh ảnh.
- Các action chưa hoạt động dùng button disabled/span `aria-disabled`, không giả làm link.
- Focus outline có độ tương phản cao.

## 16. Deferred to S10-S16

- S10: homepage hoàn chỉnh và featured category/product theo đặc tả bước sau.
- S11: product catalog, filter, sort và product detail theo kế hoạch.
- S12: search backend và trang kết quả.
- S13: login, register và authentication.
- S14: cart và checkout.
- S15-S16: order flow/history và các chức năng khách hàng tiếp theo theo roadmap.

Wishlist, review, coupon, payment và Admin UI không được triển khai trong S9.

## 17. Xác nhận phạm vi dữ liệu và Admin

- Database schema changed: **NO**.
- Migration created: **NO**.
- Seed changed: **NO**.
- Entity/DbContext changed: **NO**.
- Admin project changed: **NO**.
- S10 started: **NO**.
