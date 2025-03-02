using App.Domain.Enums;

namespace App.RestContracts.Users.Requests
{
    public class CreateCredentialRequest
    {
        public UserCredentialType KeyType { get; set; }
        public string Value { get; set; }
    }
}
