using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat.Common
{
    public class StateObject
    {
        public const int BUFFER_SIZE = 4;
        public const int COMMAND_SIZE = 4;

        public Client Client { get; set; } = new Client();

        public byte[] Command { get; set; } = new byte[COMMAND_SIZE];

        public byte[] Buffer { get; set; } = new byte[BUFFER_SIZE];
    }
}
