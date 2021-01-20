using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using Api.Data;
using Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Microsoft.EntityFrameworkCore;
using Api.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(
            DataContext context,
            ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username already exists");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(u => u.UserName == username.ToLower());
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var dbUser = await _context.Users
            .SingleOrDefaultAsync(u => u.UserName == loginDto.Username);

        if (dbUser == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(dbUser.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != dbUser.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        return new UserDto
        {
            Username = dbUser.UserName,
            Token = _tokenService.CreateToken(dbUser)
        };
    }
}
}