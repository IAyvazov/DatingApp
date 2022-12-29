using Microsoft.AspNetCore.Mvc;

using AutoMapper;

using API.DTOs;
using API.Interfaces;
using API.Extensions;
using API.Entities;
using API.Helpers;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.messageRepository = messageRepository;
            this.mapper = mapper;
        }


        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = this.User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var sender = await this.userRepository.GetUserByUsernameAsync(username);
            var recipient = await this.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName, 
                Content = createMessageDto.Content
            };

            this.messageRepository.AddMessage(message);

            if(await this.messageRepository.SaveAllAsync()) return Ok(this.mapper.Map<MessageDto>(message));

            return BadRequest("Faild to send message");
        }

        [HttpGet]

        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            messageParams.Username = this.User.GetUsername();

            var messages = await this.messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = this.User.GetUsername();

            return Ok(await this.messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = this.User.GetUsername();

            var message  = await this.messageRepository.GetMessage(id);

            if(message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();

            if(message.SenderUsername == username) message.SenderDeleted = true;

            if(message.RecipientUsername == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted)
            {
                this.messageRepository.DeleteMessage(message);
            }

            if(await this.messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}