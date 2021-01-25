using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Interfaces;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Office4U.Articles.ImportExport.Api.Extensions;
using Office4U.Articles.ImportExport.Api.Helpers;

namespace Api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UsersController(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers(
                   [FromQuery] UserParams userParams)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }
            var users = await _unitOfWork.UserRepository.GetUsersAsync(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);

            // users is of type PagedList<User> 
            // (inherits List, so it's a List of Users plus Pagination info)
            Response.AddPaginationHeader(
                users.CurrentPage,
                users.PageSize,
                users.TotalCount,
                users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            var userToReturn = _mapper.Map<MemberDto>(user);

            return userToReturn;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var user = await _userRepository.GetUserByIdAsync(int.Parse(userId));

            _mapper.Map(memberUpdateDto, user);

            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}