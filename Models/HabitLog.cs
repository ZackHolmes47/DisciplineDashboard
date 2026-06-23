using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisciplineDashboard.Models
{
    public class HabitLog
    {
        public int HabitLogID { get; set; }

        [Required]
        public int HabitID { get; set; }

        [ForeignKey("HabitID")]
        public Habit? Habit { get; set; }

        [Required]
        public string UserID { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;
        public double? ActualValue { get; set; }
        public bool Completed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
