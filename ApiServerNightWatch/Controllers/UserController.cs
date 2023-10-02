
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web;
using System.Reflection.PortableExecutable;
using System;
using System.Linq;
using Microsoft.Extensions.Primitives;
using DataAccess.DbModels.Dtos;

namespace ApiServerNightWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserData _data;
    private readonly IConfiguration _config;
    private readonly RegisterValidator _validator;

    public UserController(ILogger<UserController> logger, IUserData data, IConfiguration config, RegisterValidator validator)
    {
        _logger = logger;
        _data = data;
        _config = config;
        _validator = validator;
    }
    
    [HttpPost, Route("register")]
    public async Task<IResult> Register(UserRegisterDto req)
    {
        try
        {
            var result = _validator.Validate(req);

            if (result.IsValid == false)
            {
                var error = new ErrorModel("Ќе удалось зарегистрировать данного пользовател€");
                foreach (FluentValidation.Results.ValidationFailure er in result.Errors)
                {
                    error.Errors.Add(er.ErrorMessage);
                }

                return Results.BadRequest(error);;
            }

            User? curUser = await _data.Register(req);
            if (curUser == null)
            {
                return Results.BadRequest(new ErrorModel("Ќе удалось зарегистрировать данного пользовател€"));
            }

            string jwt = TokenManager.GenerateToken(curUser, _config);
            var output = new {curUser, jwt};
            return Results.Ok(output);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }


    }

    [HttpPost, Route("login")]
    public async Task<IResult> Login(UserLoginDto req)
    {
        try
        {
            User? curUser = await _data.Login(req);
            if(curUser == null)
            {
                return Results.BadRequest(new ErrorModel("Ќе удалось найти данного пользовател€"));
            }

            string jwt = TokenManager.GenerateToken(curUser, _config);
            var output = new { curUser, jwt };

            return Results.Ok(output);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }

    }


    
    [Authorize] //[Authorize(Policy = IdentityData.AdminUserPolicyName)]
    [RequiresClaim(ClaimTypes.Role, IdentityData.AdminClaimName)]
    [HttpGet, Route("getAll")]
    public async Task<IResult> GetAll()
    {
        try
        {
            return Results.Ok(await _data.GetUsers());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, null);
            var er = new ErrorModel(ex.Message);
            return Results.BadRequest(er);
        }
    }




}



        

