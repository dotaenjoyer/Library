using Microsoft.AspNetCore.Mvc;
using Library.ModelsORM;
using Library.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Library.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public static UserJWT userjwt = new UserJWT();
        private readonly IConfiguration _configuration;
        private readonly APIContext _context;
        public UserController(APIContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        /// <summary>
        /// Creates new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
       [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterClient(User request)
        {
            CreatePasswordHash(request.User_Password, out byte[] passwordHash, out byte[] passwordSalt);

            userjwt.Username = request.User_Email;
            userjwt.PasswordHash = passwordHash;
            userjwt.PasswordSalt = passwordSalt;
            request.User_Position = "client";
            await _context.Users.AddAsync(request);
            await _context.SaveChangesAsync();
            return Ok(userjwt);
        }
        /// <summary>
        /// Login for users
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(User user)
        {
            string token = "";
            if (_context.Users.Any(info => info.User_Email == user.User_Email && info.User_Position == "admin"))
            {
                token = CreateTokenAdmin(userjwt);
            }
            else if (_context.Users.Any(info => info.User_Email == user.User_Email && info.User_Position == "client"))
            { token = CreateTokenClient(userjwt);}
            return Ok(token);
        }
        /// <summary>
        /// Add admin
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> RegisterAdmin(User request)
        {
            CreatePasswordHash(request.User_Password, out byte[] passwordHash, out byte[] passwordSalt);

            userjwt.Username = request.User_Email;
            userjwt.PasswordHash = passwordHash;
            userjwt.PasswordSalt = passwordSalt;
            request.User_Position = "admin";
            await _context.Users.AddAsync(request);
            await _context.SaveChangesAsync();
            return Ok(userjwt);
        }
        private string CreateTokenClient(UserJWT userjwt)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userjwt.Username),
                new Claim(ClaimTypes.Role, "Client")

            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            Globals.CurUser.User_Email = userjwt.Username;
            return jwt;
        }
        private string CreateTokenAdmin(UserJWT userjwt)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userjwt.Username),
                new Claim(ClaimTypes.Role, "Admin")

            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            Globals.CurUser.User_Email = userjwt.Username;
            return jwt;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
        }
