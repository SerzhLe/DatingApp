using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly PresenceTracker _tracker;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly IUnitOfWork _unitOfWork;
        public MessageHub(IUnitOfWork unitOfWork, IMapper mapper, PresenceTracker tracker, IHubContext<PresenceHub> presenceHub)
        {
            _unitOfWork = unitOfWork;
            _presenceHub = presenceHub;
            _tracker = tracker;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUsername = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(Context.User.GetCurrentUserName(), otherUsername);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            if (group.Connections.Any(c => c.UserName == otherUsername))
                await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            (IEnumerable<MessageDto> messages, int unreadMessagesCount) messagesWithCount = await _unitOfWork.MessageRepository
                .GetMessageThread(Context.User.GetCurrentUserName(), otherUsername);

            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();

            if (messagesWithCount.unreadMessagesCount > 0)
            {
                await Clients.Caller.SendAsync("ReduceUnreadMessages", messagesWithCount.unreadMessagesCount);
            }

            await Clients.Caller.SendAsync("ReceiveMessageThread", messagesWithCount.messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromGroup();
            if (!group.Connections.Any(c => c.UserName == Context.User.GetCurrentUserName()))
                await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            //when user move disconnects - signalR automatically remove user from group
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetCurrentUserName();

            if (username == createMessageDto.RecipientUserName) throw new HubException("You cannot send messages to yourself!");

            var sender = await _unitOfWork.UserRepository.GetUserWithPhotosByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserWithPhotosByUsernameAsync(createMessageDto.RecipientUserName);

            if (recipient == null) throw new HubException("User not found");

            var message = new Message()
            {
                SenderId = sender.Id,
                SenderUserName = sender.UserName,
                RecipientId = recipient.Id,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content
            };

            //we created two entities in db and we are tracking connection id in groups in db and save in connections usernames
            //this is because we want to track if specified user has connected to group - if yes - then track the messages read
            //db needed because we cannot actually take username of current connection id
            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(c => c.UserName == message.RecipientUserName))
            {
                message.MessageRead = DateTime.UtcNow;
            }
            else
            {
                var recipientconnectionIds = await _tracker.GetUserConnectionIds(recipient.UserName);

                if (recipientconnectionIds != null)
                {
                    await _presenceHub.Clients.Clients(recipientconnectionIds)
                    //Clients are context of ONLY message hub! But we need access to presence hub to notify recipient user
                    //await Clients.All
                    //     .GroupExcept(groupName, group.Connections
                    //         .Where(c => c.UserName == username)
                    //         .Select(c => c.ConnectionId))
                        .SendAsync("NotifyAboutNewMessage", sender.UserName);
                }
            }

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            var connection = new Connection(Context.ConnectionId, Context.User.GetCurrentUserName());

            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to add connection to group");
        }

        private async Task<Group> RemoveFromGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupFromConnection(Context.ConnectionId);

            var connection = group.Connections.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);

            _unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            var stringComparer = string.CompareOrdinal(caller, other) > 0; //compare based on alphabetic

            return stringComparer ? $"{other}-{caller}" : $"{caller}-{other}";
        }
    }
}