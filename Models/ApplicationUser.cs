using Microsoft.AspNetCore.Identity;

namespace DisciplineDashboard.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string FullName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FirstName) ||
                    !string.IsNullOrWhiteSpace(LastName))
                {
                    return $"{FirstName} {LastName}".Trim();
                }

                return Email ?? UserName ?? "User";
            }
        }

        public string? ProfilePictureUrl { get; set; }

        public string Theme { get; set; } = "Light";
    }
}