using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DisciplineDashboard.Models
{
    public class Habit
    {
        public int HabitID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Category { get; set; }
        public string? Description { get; set; }
        public double? TargetValue { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
