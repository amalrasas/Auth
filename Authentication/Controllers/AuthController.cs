using Authentication.Data;
using Authentication.DTOs;
using Authentication.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext db,IConfiguration configuration)
        {
            _db = db;
           _configuration= configuration;
        }
        [HttpPost("register")]
        public ActionResult<UserEntity> Register(UserDto request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            UserEntity user = new()
            {
                username = request.Username,
                hashedPassword = hashedPassword,
            }; 
            _db.Users.Add(user);
            _db.SaveChanges();
            return Ok(user);
        }
        [HttpPost("login")]
        public ActionResult<UserEntity> Login(UserDto request)
        {
            UserEntity user = _db.Users.FirstOrDefault(u => u.username== request.Username);
          
            if(user == null)
            {
                return NotFound();
            }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.hashedPassword))
            {
                return BadRequest("Wrong password");
            }
            string token = CreateToken(user);
            return Ok(token);
        }
        private string CreateToken(UserEntity user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.username)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JWT:Token").Value!
                ));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
                );  
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
