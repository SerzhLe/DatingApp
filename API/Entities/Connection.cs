using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class Connection
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public Connection()
        {
        }

        public Connection(string connectionId, string userName)
        {
            ConnectionId = connectionId;
            UserName = userName;
        }
    }
}