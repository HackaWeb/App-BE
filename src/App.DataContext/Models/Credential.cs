using App.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.DataContext.Models
{
    [Table("Credentials", Schema="dto")]
    internal class Credential : BaseEntity
    {
        public UserCredentialType UserCredentialType { get; set; }
        public string Value { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
