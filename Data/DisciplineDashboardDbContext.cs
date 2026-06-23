using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DisciplineDashboard.Models;
using Microsoft.AspNetCore.Identity;

namespace DisciplineDashboard.Data
{
    public class DisciplineDashboardDbContext(DbContextOptions<DisciplineDashboardDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitLog> HabitLogs { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
    }
}
