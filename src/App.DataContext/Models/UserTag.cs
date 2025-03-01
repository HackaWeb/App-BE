using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace App.DataContext.Models
{
    [Table("UserTags", Schema = "dbo")]
    internal class UserTag : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [Required]
        public Guid TagId { get; set; }

        [ForeignKey(nameof(TagId))]
        public virtual Tag? Tag { get; set; }
    }
}
