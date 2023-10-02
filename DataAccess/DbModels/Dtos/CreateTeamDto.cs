namespace DataAccess.DbModels.Dtos;


public record AddToTeamDto(int UserId, int TeamId, string Password);
public record CreateTeamDto(int CaptainId, string Name, string Password);