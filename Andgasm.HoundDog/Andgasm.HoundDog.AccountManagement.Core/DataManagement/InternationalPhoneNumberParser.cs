using Andgasm.HoundDog.AccountManagement.Interfaces;
using System.Text.RegularExpressions;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class InternationalPhoneNumberParser : IParsePhoneNumber
    {
        public string ParsePhoneNumber(string phoneNumber)
        {
            // we are only supporting UK numbers just now
            // presumption made here that the phone number is a valid phone number
            //  *07939948389 - we need to prefix default country code - we presume uk when not provided
            //  *+447939948389 - no change needed
            //  *00447939948389 - we will convert this to the above format (+44)
            //  +4407939948389 - we will trim the leading zero on primary number here

            if (phoneNumber.StartsWith("00"))
            {
                // replace first instance of 00 wih +
                var regex = new Regex(Regex.Escape("00"));
                phoneNumber = regex.Replace(phoneNumber, "+", 1);
            }
            else if (phoneNumber.StartsWith("0"))
            {
                // replace first instance of 0 wih +44
                var regex = new Regex(Regex.Escape("0"));
                phoneNumber = regex.Replace(phoneNumber, "+44", 1);
            }
            if (phoneNumber.StartsWith("+440"))
            {
                // replace first instance of 0 in the primary number
                var regex = new Regex(Regex.Escape("+440"));
                phoneNumber = regex.Replace(phoneNumber, "+44", 1);
            }
            return phoneNumber;
        }
    }
}
