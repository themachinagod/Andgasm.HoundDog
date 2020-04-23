using Andgasm.HoundDog.Core.Email.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Twilio.Rest.Verify.V2.Service;

namespace Andgasm.HoundDog.Core.SMSVerification
{
    public class TwilioSMSVerification : ISMSVerification
    {
        private readonly IConfiguration _config;

        public TwilioSMSVerification(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendVerificationToPhoneNumber(string phonenumber)
        {
            var verification = await VerificationResource.CreateAsync(
                   to: phonenumber,
                   channel: "sms",
                   pathServiceSid: _config.GetSection("Twilio:VerificationServiceSID")?.Value
               );
            if (verification.Status == "pending") return true;
            return false;
        }

        public async Task<bool> VerifyPhoneNumber(string phonenumber, string token)
        {
            try
            {
                var verification = await VerificationCheckResource.CreateAsync(
                       to: phonenumber,
                       code: token,
                       pathServiceSid: _config.GetSection("Twilio:VerificationServiceSID")?.Value
                   );
                if (verification.Status == "approved") return true;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
