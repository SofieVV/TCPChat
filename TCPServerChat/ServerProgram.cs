﻿
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
        private static StringBuilder stringBuilder = new StringBuilder();
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

            var ipAddress = IPAddress.Parse("127.0.0.1");
            var localEndPoint = new IPEndPoint(ipAddress, port);
            Console.Write($"Waiting for connection...{Environment.NewLine}");
            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    allDone.WaitOne();
                    Console.WriteLine("CLIENT CONNECTED.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("CLIENT DISCONNECTED.");
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            var listener = (Socket)ar.AsyncState;
            var client = listener.EndAccept(ar);

            var state = new StateObject();
            state.Client.Socket = client;

            state.Client.Socket.BeginReceive(state.Client.ClientNameBuffer, 0, Client.NAME_SIZE, 0, new AsyncCallback(ReadClientNameCallback), state);
        }

        public static void ReadClientNameCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;

            try
            {
                stringBuilder.Append(Encoding.UTF8.GetString(state.Client.ClientNameBuffer, 0, Client.NAME_SIZE));
                state.Client.ClientName = stringBuilder.ToString().TrimEnd('\0');

                if (clientList.Any(client => client.ClientName == state.Client.ClientName))
                {
                    Array.Clear(state.Client.ClientNameBuffer, 0, Client.NAME_SIZE);
                    state.Client.Socket.Send(BitConverter.GetBytes((int)Command.Error));
                    state.Client.Socket.BeginReceive(state.Client.ClientNameBuffer, 0, Client.NAME_SIZE, 0, new AsyncCallback(ReadClientNameCallback), state);
                }
                else
                {
                    state.Client.Socket.Send(BitConverter.GetBytes((int)Command.Success));
                    clientList.Add(state.Client);
                    Console.WriteLine($"{state.Client.ClientName} has logged in.");
                    Send(state.Client, GetClientNames(clientList.ToArray()), Command.Add);
                    state.Client.Socket.BeginReceive(state.Client.FriendNameBuffer, 0, Client.NAME_SIZE, 0, new AsyncCallback(ReadChosenClientNameCallback), state);
                }

                stringBuilder.Clear();
            }
            catch (Exception)
            {
                Console.WriteLine("CLIENT DISCONNECTED.");
            }
        }

        public static void ReadChosenClientNameCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var clientSocket = state.Client.Socket;

            try
            {
                stringBuilder.Append(Encoding.UTF8.GetString(state.Client.FriendNameBuffer, 0, Client.NAME_SIZE));
                chosenClient = stringBuilder.ToString().TrimEnd('\0');
                stringBuilder.Clear();
                clientSocket.Receive(state.Buffer, StateObject.BUFFER_SIZE, SocketFlags.None);
                ReadSize(state);
            }
            catch (Exception)
            {
                clientList.RemoveAll(c => c.Socket == state.Client.Socket);
                Send(state.Client, " ", Command.Remove);
                Console.WriteLine($"{state.Client.ClientName} has logged out.");
                Console.WriteLine("CLIENT DISCONNECTED.");
            }
        }

        public static void ReadSize(StateObject state)
        {
            try
            {
                receivedMessageSize = BitConverter.ToInt32(state.Buffer, 0);
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
                state.Client.Socket.Receive(receivedMessageData, receivedMessageSize, SocketFlags.None);

                if (receivedMessageSize > 0)
                {
                    stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, receivedMessageSize));
                    content = stringBuilder.ToString();
                    stringBuilder.Clear();

                    Console.WriteLine($"{state.Client.ClientName} to {chosenClient}: {content}");
                    Send(state.Client, content, Command.Message);

                    Array.Clear(state.Client.FriendNameBuffer, 0, Client.NAME_SIZE);
                    state.Client.Socket.BeginReceive(state.Client.FriendNameBuffer, 0, Client.NAME_SIZE, 0, new AsyncCallback(ReadChosenClientNameCallback), state);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("CLIENT DISCONNECTED.");
            }
        }

        public static string GetClientNames(params Client[] clients)
        {
            return string.Join(",", clients.Select(c => c.ClientName));
        }

        public static byte[] BuildMessage(Command command, byte[] clientName, string message)
        {
            var listOfData = new List<byte>();

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
                        foreach (var connectedClient in clientList)
                        {
                            if (connectedClient == client)
                                dataToSend = BuildMessage(command, null, data);
                            else
                                dataToSend = BuildMessage(command, null, client.ClientName);
                            connectedClient.Socket.Send(dataToSend);
                        }

                        break;
                    case Command.Remove:
                        if (clientList.Count > 0)
                        {
                            dataToSend = BuildMessage(command, client.ClientNameBuffer, data);
                            foreach (var clientForUpdate in clientList)
                                clientForUpdate.Socket.Send(dataToSend);
                        }

                        break;
                    case Command.Message:
                        dataToSend = BuildMessage(command, client.ClientNameBuffer, data);
                        client.Socket.Send(dataToSend);

                        Broadcast(client, command, data);

                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Broadcast(Client client, Command command, string data)
        {
            Client receivingClient = clientList.FirstOrDefault(c => c.ClientName.Equals(chosenClient));

            byte[] newMessage = BuildMessage(command, client.ClientNameBuffer, data);
            receivingClient.Socket.Send(newMessage);
        }
    }
}