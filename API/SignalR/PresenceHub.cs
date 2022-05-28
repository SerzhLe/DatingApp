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
    //we will user WebSocket API to implement interactive communication
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
            //clients - all clients connected to hub, others - all users connected to hub except the one whi invoke this method
            //this method called when someone connects to hub
            //string - method that we will call on client, and username - is parameter that we will pass to this method

            var isOnline = await _presenceTracker.UserConnected(Context.User.GetCurrentUserName(), Context.ConnectionId);
            if (isOnline) //inform others ONLY if user first connected to site
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