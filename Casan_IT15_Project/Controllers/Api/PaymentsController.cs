using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Services;
using Casan_IT15_Project.Data;

namespace Casan_IT15_Project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ApplicationDbContext _context;

        public PaymentsController(IStripeService stripeService, ApplicationDbContext context)
        {
            _stripeService = stripeService;
            _context = context;
        }

        [Authorize]
        [HttpPost("create-checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request)
        {
            // Get CompanyId from user claims
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
            {
                return BadRequest(new { message = "User is not associated with a company." });
            }

            // Define plan amounts
            decimal amount = request.PlanName switch
            {
                "Basic Plan" => 999m,
                "Standard Plan" => 1499m,
                "Premium Plan" => 1999m,
                _ => 0
            };

            if (amount == 0) return BadRequest(new { message = "Invalid subscription plan." });

            try
            {
                var checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(companyId, request.PlanName, amount);
                return Ok(new { url = checkoutUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string stripeSignature = Request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(stripeSignature)) return BadRequest("Missing Stripe Signature");

            var result = await _stripeService.HandleWebhookAsync(json, stripeSignature);
            
            if (result) return Ok();
            return BadRequest();
        }

        [Authorize]
        [HttpGet("success-handler")]
        public async Task<IActionResult> SuccessHandler([FromQuery] string session_id)
        {
            // This endpoint is called when user returns from Stripe
            // In a real app, you might verify the session again here
            return Ok(new { message = "Payment processed successfully. Your subscription is being updated." });
        }

        [Authorize]
        [HttpGet("current-subscription")]
        public async Task<IActionResult> GetCurrentSubscription()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
            {
                return BadRequest(new { message = "User is not associated with a company." });
            }

            var company = await _context.Companies
                .Where(c => c.CompanyId == companyId)
                .Select(c => new { c.SubscriptionPlan, c.SubscriptionExpiry })
                .FirstOrDefaultAsync();

            if (company == null) return NotFound(new { message = "Company not found." });

            return Ok(company);
        }
    }

    public class CheckoutRequest
    {
        public string PlanName { get; set; } = string.Empty;
    }
}
