using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SampleRag.API.Models;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.API.Endpoints.Auth;

public static class DevJwtAuthEndpoints
{
    public static void MapDevAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/auth").WithTags("Auth");

        group.MapPost("/login", async ([FromBody] LoginModel request, JwtSettings jwtSettings) =>
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.SigningKey);
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim("sub", request.UserId),
                    new Claim("name", request.Email),
                    new Claim("email", request.Email),
                    new Claim("roles", request.Role),
                ]),
                Expires = DateTime.Now.AddMinutes(10),
                SigningCredentials = signingCredentials,
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Results.Ok(jwtToken);
        })
            .Accepts<LoginModel>("application/json")
            .Produces<string>(StatusCodes.Status200OK);
    }
}
