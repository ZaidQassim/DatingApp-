using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(logUserActivity))]  // to get last activity for user  
    [Route("api/[Controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            // to get userId that loggedIn now 
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // return info  user that loggedIn now 
            var userFromRepo = await _repo.GetUser(currentUserId, true);

            userParams.UserId = currentUserId;
            // if Userparams is not Gender 
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            // get list pages the users
            var users = await _repo.GetUSers(userParams);

            // tell client said by PageNumber and tell Header by info 
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(usersToReturn);
        }


        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;

            var user = await _repo.GetUser(id, isCurrentUser); 

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            // to valadation of user is same edit of my data
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userForRepo = await _repo.GetUser(id, true);
            _mapper.Map(userForUpdateDto, userForRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating User {id} failed on  save");
        }


        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            // to valadation of user is same edit of my data
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _repo.GetLike(id, recipientId);
            if (like != null)
                return BadRequest("You already Like this user");

            if (await _repo.GetUser(recipientId, false) == null)
                return NotFound();

            like = new like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _repo.Add<like>(like);
            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to  like user");
        }




    }
}