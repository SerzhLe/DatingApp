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
                            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                            .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientUserName == messageParams.UserName && !m.DeletedByRecipient),
                "Outbox" => query.Where(m => m.SenderUserName == messageParams.UserName && !m.DeleteBySender),
                _ => query.Where(m => m.RecipientUserName == messageParams.UserName && m.MessageRead == null && !m.DeletedByRecipient)
            };


            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<int> GetCountOfUnreadMessages(string username)
        {
            return await _context.Messages
                .Where(m => m.RecipientUserName == username && !m.DeletedByRecipient)
                .CountAsync(m => m.MessageRead == null);
        }

        public async Task<(IEnumerable<MessageDto> messages, int unreadMessagesCount)>
            GetMessageThread(string currentUserName, string recipientUserName)
        {
            int unreadMessagesCount;

            var messagesWithoutDeletion = _context.Messages //with projection we do not need to eager loade
                                    .Where(m => m.Recipient.UserName == currentUserName
                                            && m.Sender.UserName == recipientUserName //all messages that another user sent to logged in user
                                            || m.Recipient.UserName == recipientUserName
                                            && m.Sender.UserName == currentUserName  //all messages that logged in user sent to another user
                                    )
                                    .MarkUnreadAsRead(currentUserName, out unreadMessagesCount) //extension method for marking unread messages
                                    .AsQueryable();

            var messages = await messagesWithoutDeletion
                            .Where(m => m.Recipient.UserName == currentUserName && !m.DeletedByRecipient
                                || m.Recipient.UserName == recipientUserName && !m.DeleteBySender)
                            .OrderBy(m => m.MessageSent)
                            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                            .ToListAsync();

            var result = (messages, unreadMessagesCount);
            //optimization - projecting before ToListAsync!

            return result;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}