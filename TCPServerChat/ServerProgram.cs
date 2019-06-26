
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
        private static string chosenClient = string.Empty;

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

            state.client.Socket.BeginReceive(state.client.clientNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(ReadClientNameCallback), state);
        }

        public static void ReadClientNameCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;

            try
            {
                state.stringBuilder.Append(Encoding.UTF8.GetString(state.client.clientNameBuffer, 0, Client.nameSize));
                state.client.ClientName = state.stringBuilder.ToString().TrimEnd('\0');

                if (GetClientNames(clientList.ToArray()).Contains(state.client.ClientName))
                {
                    Array.Clear(state.client.clientNameBuffer, 0, Client.nameSize);
                    state.client.Socket.Send(BitConverter.GetBytes((int)Command.Error));
                    state.client.Socket.BeginReceive(state.client.clientNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(ReadClientNameCallback), state);
                }
                else
                {
                    state.client.Socket.Send(BitConverter.GetBytes((int)Command.Success));
                    clientList.Add(state.client);
                    Send(state.client, GetClientNames(clientList.ToArray()), Command.Add);
                    state.client.Socket.BeginReceive(state.client.friendNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(ReadChosenClientNameCallback), state);
                }

                state.stringBuilder.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ReadChosenClientNameCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;
            try
            {
                state.stringBuilder.Append(Encoding.UTF8.GetString(state.client.friendNameBuffer, 0, Client.nameSize));
                chosenClient = state.stringBuilder.ToString().TrimEnd('\0');
                state.stringBuilder.Clear();
                client.Receive(state.buffer, StateObject.bufferSize, SocketFlags.None);
                ReadSize(state);
            }
            catch (Exception)
            {
                clientList.RemoveAll(c => c.Socket == state.client.Socket);
                Send(state.client, " ", Command.Remove);
                Console.WriteLine("Client Disconnected.");
            }
        }

        public static void ReadSize(StateObject state)
        {
            try
            {
                receivedMessageSize = BitConverter.ToInt32(state.buffer, 0);
                receivedMessageData = new byte[receivedMessageSize];
                ReadMessage(state);
        }
            catch (Exception)
            {
            }
}
        public static void ReadMessage(StateObject state)
        {
            try
            {
                state.client.Socket.Receive(receivedMessageData, receivedMessageSize, SocketFlags.None);

            if (receivedMessageSize > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, receivedMessageSize));
                    content = state.stringBuilder.ToString();
                    state.stringBuilder.Clear();

                    Console.WriteLine(state.client.ClientName + ": " + content);
                    Send(state.client, content, Command.Message);

                    Array.Clear(state.client.friendNameBuffer, 0, Client.nameSize);
                    state.client.Socket.BeginReceive(state.client.friendNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(ReadChosenClientNameCallback), state);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }

        public static string GetClientNames(params Client[] clients)
        {
            return string.Join(",", clients.Select(c => c.ClientName));
        }

        public static byte[] BuildMessage(Command command, byte[] clientName, string message)
        {
            List<byte> listOfData = new List<byte>();

            int messageByteLength = Encoding.UTF8.GetByteCount(message);
            listOfData.AddRange(BitConverter.GetBytes(messageByteLength));
            listOfData.AddRange(BitConverter.GetBytes((int)command));
            if (clientName != null)
                listOfData.AddRange(clientName);

            listOfData.AddRange(Encoding.UTF8.GetBytes(message));

            return listOfData.ToArray();
        }

        public static void Send(Client client, string data, Command command)
        {
            try
            {
                byte[] dataToSend;

                switch (command)
                {
                    case Command.Add:
                        {
                            foreach (var connectedClient in clientList)
                            {
                                if (connectedClient == client)
                                {
                                    dataToSend = BuildMessage(command, null, data);
                                    client.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                                }
                                else
                                {
                                    dataToSend = BuildMessage(command, null, client.ClientName);
                                    connectedClient.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                                }
                            }

                            break;
                        }
                    case Command.Remove:
                        {
                            dataToSend = BuildMessage(command, client.clientNameBuffer, data);

                            foreach (var clientForUpdate in clientList)
                            {
                                clientForUpdate.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                            }

                            break;
                        }
                    case Command.Message:
                        {
                            dataToSend = BuildMessage(command, client.clientNameBuffer, data);
                            client.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);

                            Broadcast(client, command, data);

                            break;
                        }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }

        public static void Broadcast(Client client, Command command, string data)
        {
            var receivingClient = clientList.FirstOrDefault(c => c.ClientName.Equals(chosenClient));

            var newMessage = BuildMessage(command, client.clientNameBuffer, data);
            receivingClient.Socket.Send(newMessage);
        }
    }
}