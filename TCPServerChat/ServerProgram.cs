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

            if (!clientList.Contains(state.client))
            {
                state.client.Socket.BeginReceive(state.client.clientNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(ReadClientNameCallback), state);
            }

            client.BeginReceive(state.client.friendName, 0, Client.nameSize, 0, new AsyncCallback(ReadChosenClientNameCallback), state);
        }

        public static void ReadClientNameCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;

            try
            {
                state.stringBuilder.Append(Encoding.UTF8.GetString(state.client.clientNameBuffer, 0, Client.nameSize));
                state.client.ClientName = state.stringBuilder.ToString().TrimEnd('\0');

                clientList.Add(state.client);
                state.stringBuilder.Clear();

                Send(state.client, GetClientNames(clientList.ToArray()), Command.Add);
            }
            catch (Exception e)
            {

            }
        }

        public static void ReadChosenClientNameCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;

            try
            {
                state.stringBuilder.Append(Encoding.UTF8.GetString(state.client.friendName, 0, Client.nameSize));
                chosenClient = state.stringBuilder.ToString().TrimEnd('\0');
                state.stringBuilder.Clear();
                client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadSizeCallback), state);
            }
            catch (Exception e)
            {
                clientList.RemoveAll(c => c.Socket == state.client.Socket);
                Send(state.client, " ", Command.Remove);
                Console.WriteLine("Client Disconnected.");
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
                    Send(state.client, content, Command.Message);
                    client.BeginReceive(state.client.friendName, 0, Client.nameSize, 0, new AsyncCallback(ReadChosenClientNameCallback), state);
                }
            }
            catch (Exception e)
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

            listOfData.AddRange(BitConverter.GetBytes(message.Length));
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
                switch (command)
                {
                    case Command.Add:
                        {
                            foreach (var connectedClient in clientList)
                            {
                                if (connectedClient == client)
                                {
                                    byte[] dataToSend = BuildMessage(command, null, data);
                                    client.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                                }
                                else
                                {
                                    byte[] dataToSend = BuildMessage(command, null, client.ClientName);
                                    connectedClient.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                                }
                            }

                            break;
                        }
                    case Command.Remove:
                        {
                            byte[] dataToSend = BuildMessage(command, client.clientNameBuffer, data);

                            foreach (var clientForUpdate in clientList)
                            {
                                clientForUpdate.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                            }

                            break;
                        }
                    case Command.Message:
                        {
                            byte[] dataToSend = BuildMessage(command, client.clientNameBuffer, data);
                            client.Socket.Send(dataToSend, dataToSend.Length, SocketFlags.None);

                            Broadcast(client, command, data);

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("All clients disconnected.");
            }
        }

        public static void Broadcast (Client client, Command command, string data)
        {
            var receivingClient = clientList.FirstOrDefault(c => c.ClientName.Equals(chosenClient));

            if (chosenClient != client.ClientName)
            {
                var newMessage = BuildMessage(command, client.clientNameBuffer, data);
                receivingClient.Socket.Send(newMessage);
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
