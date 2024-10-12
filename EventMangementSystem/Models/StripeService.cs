using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Stripe;

namespace EventMangementSystem.Models
{
    

    public class StripeService
    {
        private readonly string apiKey = System.Configuration.ConfigurationManager.AppSettings["StripeSecretKey"];

        public StripeService()
        {
            StripeConfiguration.ApiKey = apiKey;
        }

        public async Task<Refund> ProcessRefundAsync(string chargeId)
        {
            var options = new RefundCreateOptions
            {
                Charge = chargeId,
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);
            return refund;
        }
    }

}