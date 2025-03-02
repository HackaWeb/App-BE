using App.Domain.Enums;

namespace App.Domain.Models
{
    public class Credential : BaseModel
    {
        public UserCredentialType UserCredentialType { get; set; }
        public string Value { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
