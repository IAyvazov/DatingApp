using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using API.Entities;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            var user = this.mapper.Map<AppUser>(registerDto);
           
            user.UserName = registerDto.Username.ToLower();

            var result = await this.userManager.CreateAsync(user, registerDto.Password);

            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await this.userManager.AddToRoleAsync(user, "Member");

            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.userManager.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            var result = await this.userManager.CheckPasswordAsync(user, loginDto.Password);

            if(!result) return Unauthorized("Invalid password");

            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }

        private async Task<bool> UserExists(string username) =>
         await this.userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}