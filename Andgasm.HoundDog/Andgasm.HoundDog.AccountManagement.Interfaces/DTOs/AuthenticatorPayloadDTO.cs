namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public class AuthenticatorPayloadDTO
    {
        public string SharedKey { get; set; }
        public string QrCodeUri { get; set; }
    }
}
