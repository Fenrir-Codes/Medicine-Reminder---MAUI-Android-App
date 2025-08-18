using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicinereminder.Models
{
    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<MedicineSchedule>? Schedules { get; set; }
    }
}
