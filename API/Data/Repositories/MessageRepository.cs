using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(g => g.Connections).FirstOrDefaultAsync(g => g.Name == groupName);
        }

        public async Task<Group> GetGroupFromConnection(string connectionId)
        {
            return await _context.Groups
                .Include(g => g.Connections)
                .SingleOrDefaultAsync(g => g.Connections
                    .Any(c => c.ConnectionId == connectionId));
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                            .OrderByDescending(m => m.MessageSent)
                            .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.UserName && !m.DeletedByRecipient),
                "Outbox" => query.Where(m => m.Sender.UserName == messageParams.UserName && !m.DeleteBySender),
                _ => query.Where(m => m.Recipient.UserName == messageParams.UserName && m.MessageRead == null && !m.DeletedByRecipient)
            };

            var message = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(message, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<int> GetCountOfUnreadMessages(string username)
        {
            return await _context.Messages.Where(m => m.RecipientUserName == username).CountAsync(m => m.MessageRead == null);
        }

        public async Task<(IEnumerable<MessageDto> messages, int unreadMessagesCount)>
            GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                                    .Include(m => m.Sender).ThenInclude(p => p.Photos) //because we do not projecting but loading to memory
                                    .Include(m => m.Recipient).ThenInclude(p => p.Photos)
                                    .Where(m => m.Recipient.UserName == currentUserName
                                            && m.Sender.UserName == recipientUserName //all messages that another user sent to logged in user
                                            && !m.DeletedByRecipient
                                            || m.Recipient.UserName == recipientUserName
                                            && m.Sender.UserName == currentUserName  //all messages that logged in user sent to another user
                                            && !m.DeleteBySender
                                    )
                                    .OrderBy(m => m.MessageSent)
                                    .ToListAsync();

            var unreadMessages = messages.Where(m => m.MessageRead == null && m.Recipient.UserName == currentUserName).ToList();
            var unreadMessagesCount = unreadMessages.Count;

            if (unreadMessagesCount > 0)
            {
                foreach (var message in unreadMessages) //if message was unread - make it read
                {
                    message.MessageRead = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            var result = (_mapper.Map<IEnumerable<MessageDto>>(messages), unreadMessagesCount);

            return result;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}