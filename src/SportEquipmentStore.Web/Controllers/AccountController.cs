using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Core.Models.Authentication;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet("register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterCustomerAsync(
            model.Username.Trim(),
            model.Email.Trim(),
            model.FullName.Trim(),
            model.Password,
            cancellationToken);

        if (!result.Succeeded)
        {
            AddRegistrationError(result.Failure);
            return View(model);
        }

        TempData["AuthSuccess"] = "Đăng ký thành công. Bạn có thể đăng nhập ngay bây giờ.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await _authService.AuthenticateCustomerAsync(
            model.Identifier.Trim(),
            model.Password,
            cancellationToken);
        if (customer is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tên đăng nhập/email hoặc mật khẩu không đúng.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.UserId.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, customer.Username),
            new(ClaimTypes.Role, customer.RoleName),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.GivenName, customer.FullName),
            new("CustomerId", customer.CustomerId.ToString(CultureInfo.InvariantCulture)),
        };
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }

    private void AddRegistrationError(CustomerRegistrationFailure failure)
    {
        switch (failure)
        {
            case CustomerRegistrationFailure.DuplicateUsername:
                ModelState.AddModelError(
                    nameof(RegisterViewModel.Username),
                    "Tên đăng nhập đã được sử dụng.");
                break;
            case CustomerRegistrationFailure.DuplicateEmail:
                ModelState.AddModelError(
                    nameof(RegisterViewModel.Email),
                    "Email đã được sử dụng.");
                break;
            case CustomerRegistrationFailure.CustomerRoleUnavailable:
                ModelState.AddModelError(
                    string.Empty,
                    "Hệ thống chưa thể tạo tài khoản khách hàng. Vui lòng thử lại sau.");
                break;
            default:
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể tạo tài khoản với thông tin đã cung cấp.");
                break;
        }
    }
}
