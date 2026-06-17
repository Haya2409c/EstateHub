using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RealtorsPortal.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealtorsPortal.Controllers
{
    [ApiController]
    [Route("api/auth/admin")]
    public class AdminAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AdminAuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        // ── POST /api/auth/admin/login ───────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { error = "Email and password are required." });

            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Unauthorized(new { error = "Invalid credentials." });

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                return Unauthorized(new { error = "Access denied. Admin account required." });

            var ok = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!ok)
                return Unauthorized(new { error = "Invalid credentials." });

            var (accessToken, accessExpiry) = GenerateAccessToken(user, await _userManager.GetRolesAsync(user));
            var refreshToken = GenerateRefreshToken();

            // Store refresh token hash in user security stamp slot (lightweight; swap to a DB table in prod)
            await _userManager.SetAuthenticationTokenAsync(user, "AdminPanel", "RefreshToken", refreshToken);

            return Ok(new
            {
                accessToken,
                refreshToken,
                expiresAt = accessExpiry,
                user = new { user.Id, user.FullName, user.Email, user.PhotoUrl }
            });
        }

        // ── POST /api/auth/admin/refresh ─────────────────────
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken) || string.IsNullOrWhiteSpace(req.UserId))
                return BadRequest(new { error = "userId and refreshToken are required." });

            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user is null)
                return Unauthorized(new { error = "User not found." });

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                return Unauthorized(new { error = "Access denied. Admin account required." });

            var stored = await _userManager.GetAuthenticationTokenAsync(user, "AdminPanel", "RefreshToken");
            if (stored != req.RefreshToken)
                return Unauthorized(new { error = "Invalid or expired refresh token." });

            var (accessToken, accessExpiry) = GenerateAccessToken(user, await _userManager.GetRolesAsync(user));
            var newRefreshToken = GenerateRefreshToken();

            await _userManager.SetAuthenticationTokenAsync(user, "AdminPanel", "RefreshToken", newRefreshToken);

            return Ok(new
            {
                accessToken,
                refreshToken = newRefreshToken,
                expiresAt = accessExpiry
            });
        }

        // ── POST /api/auth/admin/logout ──────────────────────
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is not null)
                    await _userManager.RemoveAuthenticationTokenAsync(user, "AdminPanel", "RefreshToken");
            }
            return Ok(new { message = "Logged out." });
        }

        // ── Helpers ──────────────────────────────────────────
        private (string token, DateTime expiry) GenerateAccessToken(
            ApplicationUser user, IList<string> roles)
        {
            var jwtSection  = _config.GetSection("Jwt");
            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!));
            var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresMins = int.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "60");
            var expiry      = DateTime.UtcNow.AddMinutes(expiresMins);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new("fullName",                    user.FullName),
                new("photoUrl",                    user.PhotoUrl ?? ""),
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer:   jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims:   claims,
                expires:  expiry,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    // ── Request DTOs ──────────────────────────────────────────
    public sealed record AdminLoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string UserId, string RefreshToken);
}
