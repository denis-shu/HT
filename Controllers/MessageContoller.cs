using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOS;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    public class MessageContoller : Controller
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessageContoller(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }


        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, MessageParams messageParams)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _repo.GetMEssageForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReternDTO>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize
            , messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId,
         [FromBody]MessageForCreationDTO messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                return BadRequest("User not found...");

            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(message);

            var messageToReturn = _mapper.Map<MessageForCreationDTO>(message);

            if (await _repo.SaveAll())
                return CreatedAtRoute("GetMessage", new { id = message.Id }, message);

            throw new Exception("Creating message failed");
        }

        [HttpGet("thread/{id}")]
        public async Task<IActionResult> GetMEssageThread(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessageThread(userId, id);

            var messThread = _mapper.Map<IEnumerable<MessageToReternDTO>>(messageFromRepo);

            return Ok(messThread);
        }
    }
}