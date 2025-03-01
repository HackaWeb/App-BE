using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.DataContext.Models
{
    [Table("Tags", Schema = "dbo")]
    internal class Tag : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;

        public virtual List<UserTag>? UserTags { get; set; }
    }
}
