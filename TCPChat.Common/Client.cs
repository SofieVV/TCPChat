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
        public const int nameSize = 20;
        public byte[] clientNameBuffer = new byte[nameSize];
        public byte[] friendNameBuffer = new byte[nameSize];

        public string ClientName { get; set; }

        public Socket Socket { get; set; }
    }
}
