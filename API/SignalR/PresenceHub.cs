using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;
        private readonly IUnitOfWork _unitOfWork;

        public PresenceHub(PresenceTracker presenceTracker, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var isOnline = await _presenceTracker.UserConnected(Context.User.GetCurrentUserName(), Context.ConnectionId);
            if (isOnline)
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetCurrentUserName());

            var onlineUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", onlineUsers);

            var unreadMessagesCount = await _unitOfWork.MessageRepository.GetCountOfUnreadMessages(Context.User.GetCurrentUserName());
            await Clients.Caller.SendAsync("UnreadMessagesCount", unreadMessagesCount);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _presenceTracker.UserDisconnected(Context.User.GetCurrentUserName(), Context.ConnectionId);

            if (isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetCurrentUserName());

            await base.OnDisconnectedAsync(exception);
        }
    }
}