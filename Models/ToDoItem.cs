using System.ComponentModel.DataAnnotations;

namespace DisciplineDashboard.Models
{
    public class TodoItem
    {
        public int TodoItemID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}