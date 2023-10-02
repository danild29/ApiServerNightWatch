using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbModels;

public class EventModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Password { get; set; }
    public required int CreatorId { get; set; }
    public required DateTime StartAt { get; set; }
    public string? Description { get; set; } 
    public int Duration { get; set; }


    public List<Team> Teams { get; set; }
}
