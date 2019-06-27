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
        public Client client = new Client();
        public const int BUFFER_SIZE = 4;
        public const int COMMAND_SIZE = 4;
        public byte[] command = new byte[COMMAND_SIZE];
        public byte[] buffer = new byte[BUFFER_SIZE];
    }
}
