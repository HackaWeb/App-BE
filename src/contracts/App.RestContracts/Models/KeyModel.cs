using App.Domain.Enums;

namespace App.RestContracts.Models
{
    public class KeyModel
    {
        public UserCredentialType KeyType { get; set; }
        public string Value { get; set; }
    }
}
