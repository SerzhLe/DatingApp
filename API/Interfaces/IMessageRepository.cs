using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
using API.DTOs;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupFromConnection(string connectionId);
        void AddMessage(Message message);

        void DeleteMessage(Message message);

        Task<Message> GetMessage(int id);

        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);

        Task<(IEnumerable<MessageDto> messages, int unreadMessagesCount)> GetMessageThread(string currentUsername, string recipientUsername);
        Task<int> GetCountOfUnreadMessages(string username);
        Task<bool> SaveAllAsync();
    }
}