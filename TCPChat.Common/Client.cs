using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat.Common
{
    public class Client
    {
        public const int NAME_SIZE = 20;

        public byte[] ClientNameBuffer { get; set; } = new byte[NAME_SIZE];

        public byte[] FriendNameBuffer { get; set; } = new byte[NAME_SIZE];

        public string ClientName { get; set; }

        public Socket Socket { get; set; }
    }
}
