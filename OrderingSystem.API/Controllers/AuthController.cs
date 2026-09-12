using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OrderingSystem.API.DTOs;
using OrderingSystem.API.DTOs.authDtos;
using OrderingSystem.Domain.Entities;
using OrderingSystem.Domain.Interfaces;

namespace OrderingSystem.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(
    UserManager<Customer> userManager,
    SignInManager<Customer> signInManager,
    ITokenService tokensService,
    ILogger<AuthController> logger,IRefreshTokenRepository refreshTokenRepository) : ControllerBase
    {
        private readonly ITokenService _tokensService=tokensService;
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

        /// <summary>Registers a new customer.</summary>
        /// <response code="201">Customer created, tokens returned.</response>
        /// <response code="400">Validation failed (includes weak-password errors from Identity).</response>
        /// <response code="409">Email already registered.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = new Customer
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                Name = request.Name.Trim(),
                PhoneNumber = request.PhoneNumber
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
                    return Conflict(new ApiErrorResponse { Message = "Email is already registered." });

                return BadRequest(new ApiErrorResponse
                {
                    Message = "Registration failed.",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }
            await userManager.AddToRoleAsync(user, nameof(Customer));
            var roles = await userManager.GetRolesAsync(user);

            var (accessToken, expiresAtUtc) = _tokensService.GenerateAccessToken(user, roles);
            var refreshToken = _tokensService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = _tokensService.GenerateRefreshTokenHash(refreshToken),
                CustomerId = user.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            logger.LogInformation("Customer {CustomerId} registered.", user.Id);

            return CreatedAtAction(nameof(Register), new AuthResponseDto
            {
                CustomerId = user.Id,
                Name = user.Name,
                Email = user.Email!,
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc,
                RefreshToken = refreshToken
            });
        }

        /// <summary>Authenticates an existing customer and issues a JWT.</summary>
        /// <response code="200">Login successful, tokens returned.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Invalid credentials.</response>
        /// <response code="403">Account temporarily banned (order-deletion rule).</response>
        /// <response code="423">Account locked out after repeated failed logins.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await userManager.FindByEmailAsync(normalizedEmail);

            if (user is null)
            {
                logger.LogWarning("Failed login attempt for {Email}.", normalizedEmail);
                return Unauthorized(new ApiErrorResponse { Message = "Invalid email or password." });
            }

            // CheckPasswordSignInAsync verifies the password AND applies Identity's own
            // lockout policy (MaxFailedAccessAttempts) — no manual attempt-counting needed.
            var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new ApiErrorResponse
                {
                    Message = $"Too many failed attempts. Try again after {user.LockoutEnd:O}."
                });
            }

            if (!signInResult.Succeeded)
            {
                logger.LogWarning("Failed login attempt for {Email}.", normalizedEmail);
                return Unauthorized(new ApiErrorResponse { Message = "Invalid email or password." });
            }

            // Separate custom rule: 3+ same-day order deletions -> 6h ordering ban.
            if (user.BannedUntil is { } bannedUntil && bannedUntil > DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
                {
                    Message = $"Account temporarily restricted from placing orders until {bannedUntil:O} UTC."
                });
            }

            var roles = await userManager.GetRolesAsync(user);
            var (accessToken, expiresAtUtc) = _tokensService.GenerateAccessToken(user, roles);
            var refreshToken = _tokensService.GenerateRefreshToken();

            logger.LogInformation("Customer {CustomerId} logged in.", user.Id);


            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = _tokensService.GenerateRefreshTokenHash(refreshToken),
                CustomerId = user.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            return Ok(new AuthResponseDto
            {
                CustomerId = user.Id,
                Name = user.Name,
                Email = user.Email!,
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc,
                RefreshToken = refreshToken,
                Roles = roles.ToList()
            });
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var tokenHash = _tokensService.GenerateRefreshTokenHash(request.RefreshToken);

            var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (refreshToken is null)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Message = "Invalid refresh token."
                });
            }

            if (!refreshToken.IsActive)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Message = "Refresh token is expired or revoked."
                });
            }

            var customer = refreshToken.Customer;

            if (customer.BannedUntil is { } bannedUntil && bannedUntil > DateTime.UtcNow)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ApiErrorResponse
                    {
                        Message =$"Account temporarily restricted until {bannedUntil:O} UTC."
                    });
            }

            var roles = await userManager.GetRolesAsync(customer);

            // Generate new access token
            var (accessToken, expiresAtUtc) =_tokensService.GenerateAccessToken(customer, roles);

            // Generate new refresh token
            var newRefreshToken = _tokensService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                TokenHash = _tokensService.GenerateRefreshTokenHash(newRefreshToken),
                CustomerId = customer.Id,
                ExpiresAtUtc =DateTime.UtcNow.AddDays(30)
            };

            // Revoke old token
            refreshToken.RevokedAtUtc = DateTime.UtcNow;

            // Save new token
            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

            logger.LogInformation(
                "Refresh token rotated for customer {CustomerId}.",
                customer.Id);

            return Ok(new AuthResponseDto
            {
                CustomerId = customer.Id,
                Name = customer.Name,
                Email = customer.Email!,
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc,
                RefreshToken = newRefreshToken
            });
        }
    }
}