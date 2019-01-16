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
  [ServiceFilter(typeof(LogUserActivity))]
  [Authorize]
  [Route("api/[controller]")]
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

    /* 
      @route    GET api/[controller]
      @desc     Get all the users
      @access   Private
    */
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
    {
      var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
      var userFromRepo = await _repo.GetUser(currentUserId);

      userParams.UserId = currentUserId;

      if (string.IsNullOrEmpty(userParams.Gender))
      {
        userParams.Gender = userFromRepo.gender == "male" ? "female" : "male";
      }

      var users = await _repo.GetUsers(userParams);
      var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

      Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

      return Ok(usersToReturn);
    }

    /* 
      @route    GET api/[controller]/id
      @desc     Get single user details
      @access   Private
    */
    [HttpGet("{id}", Name = "GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await _repo.GetUser(id);

      var userToReturn = _mapper.Map<UserForDetailedDto>(user);

      return Ok(userToReturn);
    }

    /* 
      @route    PUT api/[controller]/id
      @desc     Edits User details
      @access   Private
    */
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
    {
      // validate the user id.
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }

      //get the user details
      var userFromRepo = await _repo.GetUser(id);

      //map the user details with the field that needs to update
      _mapper.Map(userForUpdateDto, userFromRepo);

      //save changes to database
      if (await _repo.SaveAll())
      {
        return NoContent();
      }

      //if failed to save, trow an exception
      throw new Exception($"Updating user {id} failed on save");


    }
    /* 
      @route    POST api/[controller]/id/like/recipientId
      @desc     Like User 
      @access   Private
    */
    [HttpPost("{id}/like/{recipientId}")]
    public async Task<IActionResult> LikeUser(int id, int recipientId)
    {
      // validate the user id.
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      var like = await _repo.GetLike(id, recipientId);
      if (like != null)
      {
        return BadRequest("You have already liked this user");
      }

      if (await _repo.GetUser(recipientId) == null)
      {
        return NotFound();
      }

      like = new Like
      {
        LikerId = id,
        LikeeId = recipientId
      };

      _repo.Add<Like>(like);

      if (await _repo.SaveAll())
      {
        return Ok();
      }

      return BadRequest("Failed to like this user.");
    }


  }
}