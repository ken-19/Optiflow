using Stripe;
using Stripe.Checkout;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Casan_IT15_Project.Services
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(int companyId, string planName, decimal amount);
        Task<bool> HandleWebhookAsync(string json, string stripeSignature);
    }

    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ApplicationDbContext context, ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreateCheckoutSessionAsync(int companyId, string planName, decimal amount)
        {
            var domain = _configuration["AllowedHosts"] == "*" ? "https://localhost:7144" : "https://optiflowsystem101.runasp.net";
            
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100), // Stripe expects amount in cents
                            Currency = "php",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"{planName} Subscription",
                                Description = $"OptiFlow ERP-MES {planName} Monthly Plan",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + "/payment/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/pricing",
                Metadata = new Dictionary<string, string>
                {
                    { "CompanyId", companyId.ToString() },
                    { "PlanName", planName }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task<bool> HandleWebhookAsync(string json, string stripeSignature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _configuration["Stripe:WebhookSecret"]);

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null)
                    {
                        var companyIdStr = session.Metadata["CompanyId"];
                        var planName = session.Metadata["PlanName"];

                        if (int.TryParse(companyIdStr, out int companyId))
                        {
                            await UpdateCompanySubscription(companyId, planName);
                        }
                    }
                }

                return true;
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe Webhook Error");
                return false;
            }
        }

        private async Task UpdateCompanySubscription(int companyId, string planName)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company != null)
            {
                company.SubscriptionPlan = planName;
                company.SubscriptionExpiry = DateTime.UtcNow.AddMonths(1);
                company.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated subscription for Company {companyId} to {planName}");
            }
        }
    }
}
