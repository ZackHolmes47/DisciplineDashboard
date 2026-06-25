using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DisciplineDashboard.Models
{
    public class Challenge
    {
        public int ChallengeID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Precision(10, 2)]
        public decimal TargetValue { get; set; }

        [Precision(10, 2)]
        public decimal CurrentValue { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}