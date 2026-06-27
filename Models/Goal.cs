using System.ComponentModel.DataAnnotations;

namespace DisciplineDashboard.Models
{
    public class Goal
    {
        public int GoalID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public DateOnly? TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}