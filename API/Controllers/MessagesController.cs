using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            if (User.GetCurrentUserName() == createMessageDto.RecipientUserName)
                return BadRequest("You cannot send messages to yourself!");

            var sender = await _unitOfWork.UserRepository.GetUserByIdAsync(User.GetUserId());
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

            if (recipient == null) return NotFound();

            var message = new Message()
            {
                SenderId = sender.Id,
                SenderUserName = sender.UserName,
                RecipientId = recipient.Id,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            var messageDto = _mapper.Map<MessageDto>(message);

            if (await _unitOfWork.Complete()) return Ok(messageDto);

            return BadRequest("Failed to create message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetCurrentUserName();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(int messageId)
        {
            var message = await _unitOfWork.MessageRepository.GetMessage(messageId);

            var currentUserName = User.GetCurrentUserName();

            if (message == null) return NotFound();

            if (message.SenderUserName != currentUserName && message.RecipientUserName != currentUserName)
                return Unauthorized();

            if (message.SenderUserName == currentUserName) message.DeleteBySender = true;

            if (message.RecipientUserName == currentUserName) message.DeletedByRecipient = true;

            if (message.DeleteBySender && message.DeletedByRecipient) _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the message");
        }
    }
}