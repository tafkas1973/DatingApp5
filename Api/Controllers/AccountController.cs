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
using AutoMapper;

namespace Api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(
            DataContext context,
            ITokenService tokenService,
            IMapper mapper)
        {
            _tokenService = tokenService;
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username already exists");

            var user = _mapper.Map<AppUser>(registerDto);

            using var hmac = new HMACSHA512();

            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
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
                Token = _tokenService.CreateToken(dbUser),
                KnownAs = dbUser.KnownAs,
                Gender = dbUser.Gender
            };
        }
    }
}