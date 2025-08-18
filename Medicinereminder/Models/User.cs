using System.ComponentModel.DataAnnotations;

namespace Medicinereminder.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<Medicine>? Medicines { get; set; }
    }
}
