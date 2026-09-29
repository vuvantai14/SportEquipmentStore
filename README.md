# SportEquipmentStore

## Đề tài

Xây dựng hệ thống quản lý và bán dụng cụ thể thao.

## Mục tiêu

Xây dựng hệ thống gồm:

- Website dành cho khách hàng.
- Ứng dụng Desktop dành cho quản trị viên.
- Hai ứng dụng sử dụng chung dữ liệu nghiệp vụ trên SQL Server.

## Công nghệ dự kiến

- C#
- .NET 9
- ASP.NET Core MVC
- Windows Forms
- Entity Framework Core
- Microsoft SQL Server

## Cấu trúc solution

- `SportEquipmentStore.Web`: website ASP.NET Core MVC dành cho khách hàng.
- `SportEquipmentStore.Admin`: ứng dụng Windows Forms dành cho quản trị viên.
- `SportEquipmentStore.Core`: domain, model và các thành phần dùng chung; không chứa UI.
- `SportEquipmentStore.Data`: tầng truy cập dữ liệu; dự kiến sử dụng Entity Framework Core và SQL Server; không chứa UI.

## Kiến trúc tổng quát

```text
Customer
    |
ASP.NET Core MVC
    |
Core / Data
    |
SQL Server
    |
WinForms Admin
```

> Repository hiện mới ở giai đoạn chuẩn hóa nền tảng. Các chức năng nghiệp vụ và kết nối cơ sở dữ liệu sẽ được triển khai ở những giai đoạn sau.
