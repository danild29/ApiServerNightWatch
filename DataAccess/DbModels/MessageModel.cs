
namespace DataAccess.DbModels;

public class MessageModel
{
    public int Id { get; set; }
    public required int FromTeam { get; set; }
    public required int ToEvent { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
