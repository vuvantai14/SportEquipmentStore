# Sport Equipment Store

**Đề tài:** Xây dựng hệ thống quản lý và bán dụng cụ thể thao

**Môn:** Ngôn ngữ lập trình C#

Sport Equipment Store là hệ thống quản lý và bán dụng cụ thể thao, bao gồm website dành cho khách hàng và ứng dụng Windows Forms dành cho quản trị viên. Hai ứng dụng sử dụng chung dữ liệu nghiệp vụ trên Microsoft SQL Server.

## Features

### Customer Web

- Responsive customer website foundation.
- Shared header and navigation.
- Category navigation loaded from the database through the Service Layer.
- Search interface foundation.
- Account interface foundation.
- Shopping cart interface foundation.
- Responsive footer and reusable benefit section.
- Reusable product card component.
- Responsive desktop, tablet and mobile layouts.
- Local SVG icons, branding and placeholder assets.
- Basic accessibility support with semantic landmarks, labels and keyboard focus states.

### Business Layer

- Category management service.
- Product management and product search services.
- Customer profile service.
- Persistent shopping cart business logic.
- Quantity and stock validation.
- Order creation from a customer's cart.
- Transaction handling for checkout and order updates.
- Inventory deduction after successful order creation.
- Order cancellation and inventory restoration.
- Server-side order total calculation.

### Admin Application

- Windows Forms project foundation.
- Shared Core/Data architecture.
- Prepared to use the same Service Layer and SQL Server database as the customer website.

The management dashboard, authentication screens and CRUD interface for the Admin application are not complete.

## Technology Stack

### Backend / Web

- C#
- .NET 9
- ASP.NET Core MVC
- Razor Views

### Desktop

- Windows Forms

### Data

- Microsoft SQL Server
- Entity Framework Core 9.0.20

### Frontend

- HTML
- CSS
- Bootstrap
- JavaScript
- Local SVG assets

### Development

- Visual Studio Code
- Git
- GitHub

## Architecture

```text
Customer Web
     │
     ▼
Service Layer
     │
     ▼
Entity Framework Core
     │
     ▼
Microsoft SQL Server
     ▲
     │
Service Layer
     ▲
     │
Admin WinForms
```

- **SportEquipmentStore.Core:** Domain entities, enums, service interfaces and shared models.
- **SportEquipmentStore.Data:** EF Core DbContext, migrations, seed configuration and service implementations.
- **SportEquipmentStore.Web:** ASP.NET Core MVC customer website.
- **SportEquipmentStore.Admin:** Windows Forms administration application.

The Service Layer keeps shared business rules outside the Web and Admin user interfaces. UI projects do not need to work directly with `SportEquipmentStoreDbContext` for the supported operations.

## Project Structure

```text
SportEquipmentStore/
├── docs/
├── src/
│   ├── SportEquipmentStore.Admin/
│   ├── SportEquipmentStore.Core/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── Models/
│   ├── SportEquipmentStore.Data/
│   │   ├── Context/
│   │   ├── Migrations/
│   │   ├── Seed/
│   │   └── Services/
│   └── SportEquipmentStore.Web/
│       ├── Controllers/
│       ├── Models/
│       ├── ViewComponents/
│       ├── Views/
│       └── wwwroot/
├── SportEquipmentStore.sln
└── README.md
```

Generated `bin/` and `obj/` directories are not part of the repository structure shown above.

## Database

- **Database name:** `SportEquipmentStoreDb`
- **Database engine:** Microsoft SQL Server
- **Development endpoint:** `tcp:localhost,1433`
- **Authentication in the current development configuration:** Windows Authentication

The database contains the following business tables:

- `Roles`
- `Users`
- `Customers`
- `Categories`
- `Products`
- `Carts`
- `CartItems`
- `Orders`
- `OrderDetails`

EF Core records applied migrations in `__EFMigrationsHistory`. The current schema is represented by the `InitialCreate` migration.

No password or SQL credential is stored in this README. Use local configuration or the .NET secret/configuration mechanism when a development environment requires a different connection string.

### Main Relationships

```text
Role       1 ─── N     User
User       1 ─── 0..1  Customer
Category   1 ─── N     Product
Customer   1 ─── 0..1  Cart
Cart       1 ─── N     CartItem
Product    1 ─── N     CartItem
Customer   1 ─── N     Order
Order      1 ─── N     OrderDetail
Product    1 ─── N     OrderDetail
```

### Initial Data

The migration provides deterministic initial data:

**Roles**

- Admin
- Customer

**Categories**

- Bóng đá
- Cầu lông
- Bóng rổ
- Tennis
- Gym & Fitness
- Bơi lội

**Products**

- 12 sample sport products distributed across the six categories.

No user password or Admin account is included in the seed data.

## Service Layer

| Interface | Implementation | Responsibility |
|---|---|---|
| `ICategoryService` | `CategoryService` | Category queries, create/update operations and active state management. |
| `IProductService` | `ProductService` | Product queries, search, validation and active state management. |
| `ICustomerService` | `CustomerService` | Customer profile queries and updates. |
| `ICartService` | `CartService` | Persistent cart, item quantity and stock validation operations. |
| `IOrderService` | `OrderService` | Order queries, checkout, status transitions and cancellation. |

The Service Layer contains reusable business logic so that Web and Admin code do not need to process supported operations directly with the DbContext.

```text
UI → Service Layer → Entity Framework Core → SQL Server
```

## Business Rules

- Product and Category use `IsActive` to represent their current availability.
- A product is visible to customers only when both the Product and its Category are active.
- Shopping carts and cart items are persisted in SQL Server.
- Cart quantities must be positive and cannot exceed available stock.
- A cart does not reserve inventory or lock a product price.
- `OrderDetail.UnitPrice` stores the product price at the time the order is placed.
- `Order.TotalAmount` is calculated by the Service Layer from the order details.
- A total supplied by the client is never trusted.
- Creating an Order, creating its OrderDetails, deducting stock and clearing CartItems are performed in one transaction.
- Inventory is deducted only when the order is created successfully.
- A valid cancellation restores inventory according to the current order rules and does not restore it more than once.
- Supported order flow is `Pending → Confirmed → Shipping → Completed`, with cancellation allowed only from valid states.

## Getting Started

### Requirements

- .NET 9 SDK
- Microsoft SQL Server
- Git
- EF Core CLI tools for applying migrations

### Clone

```powershell
git clone https://github.com/vuvantai14/SportEquipmentStore.git
cd SportEquipmentStore
```

### Restore

```powershell
dotnet restore SportEquipmentStore.sln
```

### Configure the Database

The default development configuration expects SQL Server at `tcp:localhost,1433`, database `SportEquipmentStoreDb`, using Windows Authentication. If your SQL Server setup is different, provide a local `DefaultConnection` configuration without committing credentials to Git.

Apply the existing migration:

```powershell
dotnet ef database update --project src/SportEquipmentStore.Data --startup-project src/SportEquipmentStore.Web
```

### Build

```powershell
dotnet build SportEquipmentStore.sln --no-restore
```

### Run the Customer Website

```powershell
dotnet run --project src/SportEquipmentStore.Web
```

The included launch profiles use:

- `https://localhost:7105`
- `http://localhost:5141`

### Run the Admin Foundation

The Windows Forms project requires Windows:

```powershell
dotnet run --project src/SportEquipmentStore.Admin
```

The Admin application currently provides only the project foundation; its management UI is not complete.

## Current Limitations

- Product catalog, filtering, sorting and product detail pages are not complete.
- Search has an interface foundation but no search results workflow in the Web UI.
- Login, registration and authentication are not implemented.
- Cart, checkout and order history business services exist, but their customer-facing pages are not implemented.
- Admin login, dashboard, management forms, statistics and reports are not implemented.
- Online payment, wishlist, review, coupon and promotion modules are not implemented.
