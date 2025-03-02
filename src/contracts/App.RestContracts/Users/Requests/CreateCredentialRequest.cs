using App.Domain.Enums;

namespace App.RestContracts.Users.Requests
{
    public class CreateCredentialRequest
    {
        public UserCredentialType Type { get; set; }
        public string Value { get; set; }
    }
}
