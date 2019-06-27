﻿using System;
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
        public const int bufferSize = 4;
        public const int enumCommand = 4;
        public byte[] command = new byte[enumCommand];
        public byte[] buffer = new byte[bufferSize];
    }
}
