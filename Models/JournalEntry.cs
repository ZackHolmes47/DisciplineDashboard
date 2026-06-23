using System.ComponentModel.DataAnnotations;

namespace DisciplineDashboard.Models
{
    public class JournalEntry
    {
        public int JournalEntryID { get; set; }

        [Required]
        public string UserID { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? MorningMission { get; set; }

        [StringLength(500)]
        public string? Gratitude { get; set; }

        [StringLength(1000)]
        public string? Reflection { get; set; }

        [StringLength(1000)]
        public string? WhatToImprove { get; set; }

        [StringLength(50)]
        public string? Mood { get; set; }

        [StringLength(500)]
        public string? TomorrowMission { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
