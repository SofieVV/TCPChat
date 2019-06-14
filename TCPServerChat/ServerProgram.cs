using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPChat.Common;

namespace TCPServerChat
{
    class ServerProgram
    {
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static List<Client> clientList = new List<Client>();
        private static string content = string.Empty;
        private const int port = 1020;
        private static int receivedMessageSize = 0;
        private static byte[] receivedMessageData = null;

        static void Main(string[] args)
        {
            StartListening();
        }


        public static void StartListening()
        {

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.Write($"Waiting for connection...{Environment.NewLine}");

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    allDone.WaitOne();
                    Console.WriteLine("Client Connected.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket client = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.client.Socket = client;

            if (!clientList.Contains(state.client))
            {
                state.client.Socket.BeginReceive(state.clientNameBuffer, 0, StateObject.nameSize, 0, new AsyncCallback(ReadClientNameCallback), state);
            }

            state.client.Socket.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadSizeCallback), state);
        }

        public static void ReadClientNameCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;

            try
            {
                int bytesRead = client.EndReceive(ar);

                state.stringBuilder.Append(Encoding.UTF8.GetString(state.clientNameBuffer, 0, bytesRead));
                state.client.ClientName = state.stringBuilder.ToString();

                clientList.Add(state.client);

                Send(state, " has connected.", Command.Add);
                state.stringBuilder.Clear();
            }
            catch (Exception)
            {

            }
        }
        public static void ReadSizeCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;

            try
            {
                receivedMessageSize = BitConverter.ToInt32(state.buffer, 0);
                receivedMessageData = new byte[receivedMessageSize];
                client.BeginReceive(receivedMessageData, 0, receivedMessageSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception)
            {
                clientList.RemoveAll(c => c.Socket == client);
                Send(state, " has disconnected.", Command.Remove);
                Console.WriteLine("Client Disconnected.");
            }
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;

            try
            {
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, bytesRead));
                    content = ": " + state.stringBuilder.ToString();
                    state.stringBuilder.Clear();

                    Console.WriteLine(state.client.ClientName + content);
                    Send(state, content, Command.Updated);
                    client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadSizeCallback), state);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }
        public static void Send(StateObject state, string data, Command command)
        {
            List<byte> listOfData = new List<byte>();

            listOfData.AddRange(state.clientNameBuffer);
            listOfData.AddRange(BitConverter.GetBytes((int)command));
            listOfData.AddRange(BitConverter.GetBytes(data.Length));
            listOfData.AddRange(Encoding.UTF8.GetBytes(data));
            var dataToSend = listOfData.ToArray();

            if (command == Command.Updated)
            {
                state.client.Socket.BeginSend(dataToSend, 0, dataToSend.Length, 0, new AsyncCallback(SendCallback), state.client.Socket);
                Broadcast(state.client.Socket, dataToSend);
            }
            else
            {
                foreach (var client in clientList)
                {
                    client.Socket.BeginSend(dataToSend, 0, dataToSend.Length, 0, new AsyncCallback(SendCallback), client.Socket);
                }
            }
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }

        public static void Broadcast (Socket currentClient, byte[] messageData)
        {
            foreach (var client in clientList)
            {
                if (client.Socket != currentClient)
                    client.Socket.BeginSend(messageData, 0, messageData.Length, 0, new AsyncCallback(BroadcastCallback), client.Socket);
            }
        }

        public static void BroadcastCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }
    }
}
