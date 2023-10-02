using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;

namespace ApiServerNightWatch.Auth;

public class TokenManager
{

    private static readonly TimeSpan TokenLifeTime = TimeSpan.FromDays(1);

    public static string GenerateToken(User user, IConfiguration config)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!);

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (ClaimTypes.Role, user.Role.ToString()),
                new (ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifeTime),
                Issuer = config["JwtSettings:Issuer"],
                Audience = config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
        catch
        {
            throw;
        }
    }

    public static int ParseToken(HttpRequest httpRequest)
    {
        try
        {
            string? jwt = null;

            if (httpRequest.Headers.TryGetValue("Authorization", out StringValues values))
            {
                jwt = ((string)values!).Replace("Bearer ", "");
            }
            if (jwt == null) throw new AuthenticationException("Отсутсвует токен.");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(jwt);
            var encoded = jwtToken.Claims.First(claim => claim.Type == "nameid").Value;

            return int.Parse(encoded);
        }
        catch (Exception)
        {

            throw;
        }
        

    }
}
