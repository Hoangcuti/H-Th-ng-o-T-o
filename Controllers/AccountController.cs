using System.Security.Claims;
using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Controllers;

public class AccountController : Controller
{
    private readonly TrainingContext _context;

    public AccountController(TrainingContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.UserProfiles)
            .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.FullName),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.MobilePhone, user.UserProfiles?.FirstOrDefault()?.Phone ?? "")
        };

        foreach (var role in user.UserRoles.Select(r => r.Role?.Name).Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _context.Users.AnyAsync(u => u.Username == model.Username);
        if (exists)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Username), "Tên đăng nhập đã tồn tại.");
            return View(model);
        }

        var user = new User
        {
            Username = model.Username,
            Password = model.Password, // demo only, not hashed
            Email = model.Email,
            FullName = model.FullName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // profile
        _context.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            Phone = model.Phone
        });
        await _context.SaveChangesAsync();

        // gán vai trò mặc định Student
        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole != null)
        {
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = studentRole.Id });
            await _context.SaveChangesAsync();
        }

        TempData["Registered"] = "Đăng ký thành công, hãy đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var user = await _context.Users
            .Include(u => u.UserProfiles)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        
        if (user == null) return NotFound();

        var enrolledCourses = await _context.ClassStudents
            .Include(cs => cs.Class)!.ThenInclude(c => c.Course)
            .Where(cs => cs.UserId == user.Id)
            .Select(cs => cs.Class!.Course!.CourseName)
            .Distinct()
            .ToListAsync();

        if (user == null) return NotFound();

        var vm = new ProfileViewModel
        {
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            Phone = user.UserProfiles?.FirstOrDefault()?.Phone,
            StudentCode = user.StudentCode,
            EnrolledCourses = enrolledCourses
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) 
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdStr)) {
                var u = await _context.Users.FirstOrDefaultAsync(x => x.Id.ToString() == userIdStr);
                if (u != null) {
                    model.StudentCode = u.StudentCode;
                    model.EnrolledCourses = await _context.ClassStudents
                        .Include(cs => cs.Class)!.ThenInclude(c => c.Course)
                        .Where(cs => cs.UserId == u.Id)
                        .Select(cs => cs.Class!.Course!.CourseName)
                        .Distinct()
                        .ToListAsync();
                }
            }
            return View(model);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var user = await _context.Users
            .Include(u => u.UserProfiles)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            user.Password = model.NewPassword;
        }

        var profile = user.UserProfiles?.FirstOrDefault();
        if (profile == null)
        {
            _context.UserProfiles.Add(new UserProfile { UserId = user.Id, Phone = model.Phone });
        }
        else
        {
            profile.Phone = model.Phone;
        }

        await _context.SaveChangesAsync();

        // Update Claims dynamically by re-signing in
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.FullName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.MobilePhone, model.Phone ?? "")
        };

        // Note: keeping existing roles
        foreach (var roleClaim in User.FindAll(ClaimTypes.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = true });

        TempData["Message"] = "Đã cập nhật hồ sơ thành công.";
        return RedirectToAction("Profile");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
