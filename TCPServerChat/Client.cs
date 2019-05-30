using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerChat
{
    class Client
    {
        public string ClientName { get; set; }

        public Socket ClientSocket { get; set; }
    }
}
