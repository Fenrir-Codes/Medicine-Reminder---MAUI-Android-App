using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medicinereminder.Models
{
    public class MedicineSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [ForeignKey("Medicine")]
        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }

        [Required]
        public DayOfWeek Day { get; set; }

        // Több időpontot tárolunk egy mezőben, stringként pl. "08:00,12:00,18:00"
        [Required]
        public string Times { get; set; } = string.Empty;

        [Required]
        public string Dosage { get; set; } = string.Empty;
    }
}
