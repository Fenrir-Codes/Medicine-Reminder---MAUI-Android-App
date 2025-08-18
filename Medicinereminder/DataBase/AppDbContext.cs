using Medicinereminder.Models;
using Microsoft.EntityFrameworkCore;

namespace Medicinereminder.DataBase
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicineSchedule> MedicineSchedules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "medicinreminder.db3");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
    }
}
