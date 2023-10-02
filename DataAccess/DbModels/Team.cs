using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbModels;

public class Team
{
    public int Id { get; set; }
    public required int? CaptainId { get; set; }
    public int? EventId { get; set; }
    public required string Name { get; set; }
    public required string Password { get; set; }
    public List<User>? Players { get; set; }
}
