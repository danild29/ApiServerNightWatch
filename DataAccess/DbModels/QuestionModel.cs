using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbModels;

public class QuestionModel
{
    public int Id { get; set; }
    public required int EventId { get; set; }
    public required string Answer { get; set; }
    public required string Question { get; set; }
    public string? ContentType { get; set; }
    public byte[]? Content { get; set; }

}
