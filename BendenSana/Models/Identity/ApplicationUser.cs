using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    // SQL'de Users.first_name / last_name / phone / profile_image_url vardý :contentReference[oaicite:1]{index=1}
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = default!;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = default!;

    [MaxLength(255)]
    public string? ProfileImageUrl { get; set; }

    // SQL'de created_at vardý
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Address { get; set; } 

    // Ýstersen (SQL’de role vardý) ama önerim: IdentityRole kullanýn
    // public UserRole? Role { get; set; }
}
