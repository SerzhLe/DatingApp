using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<Message> MarkUnreadAsRead(this IQueryable<Message> query, string currentUsername, out int unreadMessagesCount)
        {
            var unreadMessages = query.Where(m => m.MessageRead == null && m.RecipientUserName == currentUsername);
            unreadMessagesCount = unreadMessages.Count();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.MessageRead = DateTime.UtcNow;
                }
            }

            return query;
        }
    }
}