using Microsoft.AspNetCore.Mvc;
using Casan_IT15_Project.DTOs.Auth;
using Casan_IT15_Project.Services;

namespace Casan_IT15_Project.Controllers.Api
{
    /// <summary>
    /// Authentication endpoints — login and register.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IWebHostEnvironment env)
        {
            _authService = authService;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Authenticate a user and return a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                    return Unauthorized(new { message = "Invalid username or password" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", loginDto.Username);

                if (ex.Message.Contains("Subscription"))
                {
                    return StatusCode(403, new { message = ex.Message });
                }

                // In Development, include the real error so we can debug
                var errorMessage = _env.IsDevelopment()
                    ? $"Login error: {ex.Message}"
                    : "An error occurred during login.";

                return StatusCode(500, new { message = errorMessage });
            }
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
                return Conflict(new { message = "Username or email already exists" });

            return CreatedAtAction(nameof(Login), result);
        }
        /// <summary>
        /// Register a new tenant (Company + Admin).
        /// </summary>
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterTenantDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Basic validation for subscription plans
            var validPlans = new[] { "Basic Plan", "Standard Plan", "Premium Plan" };
            if (!validPlans.Contains(registerDto.SubscriptionPlan))
            {
                return BadRequest(new { message = "Invalid subscription plan. Choose 'Basic Plan', 'Standard Plan', or 'Premium Plan'." });
            }

            var result = await _authService.RegisterTenantAsync(registerDto);
            if (result == null)
                return Conflict(new { message = "Username or email already exists" });

            return CreatedAtAction(nameof(Login), result);
        }
        /// <summary>
        /// Emergency endpoint to seed database in production if migrations didn't run.
        /// </summary>
        [HttpGet("seed-db")]
        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                var result = await _authService.EmergencySeedAsync();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Seeding failed: " + ex.Message });
            }
        }
    }
}
