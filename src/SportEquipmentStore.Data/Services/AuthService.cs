using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Core.Models.Authentication;
using SportEquipmentStore.Data.Context;

namespace SportEquipmentStore.Data.Services;

public sealed class AuthService : IAuthService
{
    private const string CustomerRoleName = "Customer";
    private readonly SportEquipmentStoreDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        SportEquipmentStoreDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<CustomerRegistrationResult> RegisterCustomerAsync(
        string username,
        string email,
        string fullName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = ServiceValidation.RequiredText(username, 50, nameof(username));
        var normalizedEmail = ServiceValidation.RequiredText(email, 254, nameof(email));
        var normalizedFullName = ServiceValidation.RequiredText(fullName, 150, nameof(fullName));
        ValidatePassword(password);

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        if (await UsernameExistsAsync(normalizedUsername, cancellationToken))
        {
            return CustomerRegistrationResult.Failed(
                CustomerRegistrationFailure.DuplicateUsername);
        }

        if (await EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return CustomerRegistrationResult.Failed(
                CustomerRegistrationFailure.DuplicateEmail);
        }

        var customerRole = await _context.Roles
            .SingleOrDefaultAsync(
                role => role.RoleName == CustomerRoleName,
                cancellationToken);
        if (customerRole is null)
        {
            return CustomerRegistrationResult.Failed(
                CustomerRegistrationFailure.CustomerRoleUnavailable);
        }

        var user = new User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            FullName = normalizedFullName,
            RoleId = customerRole.RoleId,
            Role = customerRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        user.Customer = new Customer { User = user };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return CustomerRegistrationResult.Success;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return CustomerRegistrationResult.Failed(
                GetUniqueConstraintFailure(exception));
        }
    }

    public async Task<AuthenticatedCustomer?> AuthenticateCustomerAsync(
        string identifier,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentifier = ServiceValidation.RequiredText(
            identifier,
            254,
            nameof(identifier));
        if (string.IsNullOrEmpty(password))
        {
            return null;
        }

        var user = await UsersForAuthentication()
            .SingleOrDefaultAsync(
                item => item.Username == normalizedIdentifier,
                cancellationToken);
        user ??= await UsersForAuthentication()
            .SingleOrDefaultAsync(
                item => item.Email == normalizedIdentifier,
                cancellationToken);

        if (user is null
            || !user.IsActive
            || !string.Equals(
                user.Role.RoleName,
                CustomerRoleName,
                StringComparison.OrdinalIgnoreCase)
            || user.Customer is null)
        {
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new AuthenticatedCustomer
        {
            UserId = user.UserId,
            CustomerId = user.Customer.CustomerId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.Role.RoleName,
        };
    }

    private IQueryable<User> UsersForAuthentication()
    {
        return _context.Users
            .Include(user => user.Role)
            .Include(user => user.Customer);
    }

    private Task<bool> UsernameExistsAsync(
        string username,
        CancellationToken cancellationToken)
    {
        return _context.Users.AnyAsync(
            user => user.Username == username,
            cancellationToken);
    }

    private Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return _context.Users.AnyAsync(
            user => user.Email == email,
            cancellationToken);
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length is < 8 or > 100)
        {
            throw new ArgumentException(
                "Password must contain between 8 and 100 characters.",
                nameof(password));
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }

    private static CustomerRegistrationFailure GetUniqueConstraintFailure(
        DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? string.Empty;
        if (message.Contains("UX_Users_Username", StringComparison.Ordinal))
        {
            return CustomerRegistrationFailure.DuplicateUsername;
        }

        if (message.Contains("UX_Users_Email", StringComparison.Ordinal))
        {
            return CustomerRegistrationFailure.DuplicateEmail;
        }

        return CustomerRegistrationFailure.DuplicateAccount;
    }
}
