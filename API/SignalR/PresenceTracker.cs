using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    //it will be a store for users that are online
    //this approach is OK for a single server

    //IMPORTANT!! This service must be a single services among all connections!
    public class PresenceTracker
    {
        //first type - its users' username, second - their string id - 
        //why list? because one user can connect from differenc devices and for each device will be another string
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isFirstConnected = false;
            //here we need to be careful
            //it two users at the same time will update dictionary - we will have problems
            //use lock! to prevent unpredictable work with dictionary
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string>() { connectionId });
                    isFirstConnected = true;
                }
            }

            return Task.FromResult(isFirstConnected);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isLastDisconnected = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isLastDisconnected);

                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count <= 0)
                {
                    OnlineUsers.Remove(username);
                    isLastDisconnected = true;
                }
            }

            return Task.FromResult(isLastDisconnected);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;

            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetUserConnectionIds(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username); //default - null
            }

            return Task.FromResult(connectionIds);
        }
    }
}