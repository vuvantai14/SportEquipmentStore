using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Data.Context;

namespace SportEquipmentStore.Data.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly SportEquipmentStoreDbContext _context;

    public CustomerService(SportEquipmentStoreDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await CustomersWithUser()
            .OrderBy(customer => customer.User.FullName)
            .ToListAsync(cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(id, nameof(id));

        return CustomersWithUser()
            .SingleOrDefaultAsync(customer => customer.CustomerId == id, cancellationToken);
    }

    public Task<Customer?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(userId, nameof(userId));

        return CustomersWithUser()
            .SingleOrDefaultAsync(customer => customer.UserId == userId, cancellationToken);
    }

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        if (customer.CustomerId != 0)
        {
            throw new ArgumentException("A new customer cannot already have an identifier.", nameof(customer));
        }

        ServiceValidation.RequirePositiveId(customer.UserId, nameof(customer.UserId));
        Normalize(customer);

        var user = await _context.Users
            .Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.UserId == customer.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {customer.UserId} was not found.");

        if (!string.Equals(user.Role.RoleName, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only a user with the Customer role can own a customer profile.");
        }

        if (await _context.Customers.AnyAsync(
                item => item.UserId == customer.UserId,
                cancellationToken))
        {
            throw new InvalidOperationException($"User {customer.UserId} already has a customer profile.");
        }

        customer.User = user;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ServiceValidation.RequirePositiveId(customer.CustomerId, nameof(customer.CustomerId));
        Normalize(customer);

        var existing = await _context.Customers
            .Include(item => item.User)
            .ThenInclude(user => user.Role)
            .SingleOrDefaultAsync(item => item.CustomerId == customer.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {customer.CustomerId} was not found.");

        if (customer.UserId != existing.UserId)
        {
            throw new InvalidOperationException("A customer profile cannot be reassigned to another user.");
        }

        existing.Phone = customer.Phone;
        existing.Address = customer.Address;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private IQueryable<Customer> CustomersWithUser()
    {
        return _context.Customers
            .AsNoTracking()
            .Include(customer => customer.User)
            .ThenInclude(user => user.Role);
    }

    private static void Normalize(Customer customer)
    {
        customer.Phone = ServiceValidation.OptionalText(
            customer.Phone,
            20,
            nameof(customer.Phone));
        customer.Address = ServiceValidation.OptionalText(
            customer.Address,
            500,
            nameof(customer.Address));
    }
}
