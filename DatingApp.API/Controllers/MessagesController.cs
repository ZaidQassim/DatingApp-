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
    [Route("api/users/{userId}/[Controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            // to get user that loggedIn now 
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // to  get the message  
            var messsageFormRepo = await _repo.GetMessage(id);
            if (messsageFormRepo == null)
                return NotFound();

            return Ok(messsageFormRepo);
        }


        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            // to get user that loggedIn now 
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;

            var messageFormRepo = await _repo.GetMesssageForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFormRepo);

            Response.AddPagination(messageFormRepo.CurrentPage, messageFormRepo.PageSize,
                    messageFormRepo.TotalCount, messageFormRepo.TotalPages);

            return Ok(messages);

        }


        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            // to get user that loggedIn now 
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

            return Ok(messageThread);
        }


        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId, false);

            // to get user that loggedIn now 
            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // userId is sender 
            messageForCreationDto.SenderId = userId;

            // get info for recipient 
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId, false);
            if (recipient == null)
                return BadRequest("Could not find user ");

            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(message);



            if (await _repo.SaveAll())
            {
                // to return just info messaage  no info sender user and no recipient user
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
            }

            throw new Exception("Creating the message failed om save");




        }



        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            // to get user that loggedIn now 
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFormRepo = await _repo.GetMessage(id);

            // if deleted from sender 
            if (messageFormRepo.SenderId == userId)
                messageFormRepo.SenderDeleted = true;

            // if deleted from recipient 
            if (messageFormRepo.RecipientId == userId)
                messageFormRepo.RecipientDeleted = true;

            // if deleted from sender and recipient == delete from database 
            if (messageFormRepo.SenderDeleted && messageFormRepo.RecipientDeleted)
                _repo.Delete(messageFormRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");

        }



        [HttpPost("{id}/read")]   
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            // to get user that loggedIn now    
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if (message.RecipientId != userId)
                return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;
            await _repo.SaveAll();

            return NoContent();
        }








    }
}