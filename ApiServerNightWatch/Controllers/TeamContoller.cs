using DataAccess.DbModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;

namespace ApiServerNightWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamContoller : ControllerBase
{
    private readonly ILogger<TeamContoller> _logger;
    private readonly ITeamData _data;
    private readonly IUserData _userData;

    public TeamContoller(ILogger<TeamContoller> logger, ITeamData data, IUserData userData)
    {
        _logger = logger;
        _data = data;
        _userData = userData;
    }

    [HttpPost, Route("create")]
    public async Task<IResult> Create(CreateTeamDto team)
    {
        try 
        {
            int id = TokenManager.ParseToken(Request);

            Team? newTeam = await _data.CreateTeam(team);
            if (newTeam == null)
            {
                _logger.LogError("registered team that is null");
                return Results.Problem();
            }
            await _userData.UpdateUser(id, "TeamId", newTeam.Id);

            var user = await _userData.GetUser(team.CaptainId).ConfigureAwait(false);
            newTeam.Players?.Add(user);
            return Results.Ok(newTeam);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }


    [HttpPost, Route("add")]
    public async Task<IResult> AddTeamate(AddToTeamDto req)
    {
        try
        {
            string? error = await _data.CheckTeamPassword(req.TeamId, req.Password);
            if (error != null)
                return Results.BadRequest(new ErrorModel(error));

            await _userData.UpdateUser(req.UserId, "TeamId", req.TeamId);

            Team team = (await _data.GetTeam(req.TeamId))!;

            if(team.CaptainId == null)
            {
                await _data.UpdateTeam(team.Id, "CaptainId", req.UserId);
            }

            team.Players = (await _userData.GetAllTeamates(team.Id)).ToList();


            return Results.Ok(team);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }

    }



    [Authorize]
    [HttpPost, Route("remove")]
    public async Task<IResult> RemoveTeamate()
    {
        try
        {

            int id = TokenManager.ParseToken(Request);
            User? u = await _userData.GetUser(id);

            if (u == null) return Results.BadRequest(new ErrorModel("user not found"));
            if (u.TeamId == null) return Results.BadRequest(new ErrorModel("user's team not found"));

            int ID = (int)u.TeamId;
            var team = await _data.GetTeam(ID);


            await _userData.UpdateUser<string?>(id, "TeamId", value: null);


            if (team!.CaptainId == id)
            {
                User? teamate = (await _userData.GetAllTeamates(team.Id)).FirstOrDefault();
                if(teamate.Id == 0)
                {
                    // delete team
                    await _data.UpdateTeam<int?>(team.Id, "CaptainId", null);
                }
                else
                {
                    await _data.UpdateTeam(ID, "CaptainId", teamate);
                }
            }

            return Results.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }

    [HttpPost, Route("joinEvent")]
    public async Task<IResult> JoinEvent(int teamid, int eventId)
    {
        try
        {
            await _data.UpdateTeam(teamid, "EventId", eventId);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }

    [HttpPost, Route("exitEvent")]
    public async Task<IResult> ExitEvent(int teamid)
    {
        try
        {
            await _data.UpdateTeam<string>(teamid, "EventId", null);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }



    [Authorize]
    [HttpGet, Route("getMyTeam")]
    public async Task<IResult> GetMyTeam()
    {
        try
        {
            int id = TokenManager.ParseToken(Request);
            User? user = await _userData.GetUser(id);

            if(user!.TeamId == null)
                return Results.BadRequest(new ErrorModel("вы не зарегистрированы в команде"));

            var team = await _data.GetTeam((int)user!.TeamId!);

            team!.Players = (await _userData.GetAllTeamates(team.Id)).ToList();

            return Results.Ok(team);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
        
    }

    [Authorize] 
    [RequiresClaim(ClaimTypes.Role, IdentityData.AdminClaimName)]
    [HttpGet, Route("getAll")]
    public async Task<IResult> GetAll()
    {
        try
        {
            var teams = (await _data.GetTeams()).ToList();

            teams.ForEach(t =>
            {
                t.Players = _userData.GetAllTeamates(t.Id).Result.ToList();
            });

            return Results.Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
        
    }


    
}

