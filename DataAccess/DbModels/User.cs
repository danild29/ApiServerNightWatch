using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbModels;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool IsPremium { get; set; } = false;
    public string Role { get; set; } = "user";
    public string? AvatarUrl { get; set; } 
    public int? TeamId { get; set; }
}
