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
        private static List<Socket> clientList = new List<Socket>();
        private static string content = string.Empty;
        private const int port = 1020;
        private const int fixedSize = 4;
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
            state.workSocket = client;
            client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadSizeCallback), state);

            clientList.Add(client);
        }

        public static void ReadSizeCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            try
            {
                receivedMessageSize = BitConverter.ToInt32(state.buffer, 0);
                receivedMessageData = new byte[receivedMessageSize];
                client.BeginReceive(receivedMessageData, 0, receivedMessageSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine("Client Disconnected.");
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            try
            {
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, bytesRead));
                    content = state.stringBuilder.ToString();
                    state.stringBuilder.Clear();

                    Console.WriteLine(content);
                    Send(client, content);
                    client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadSizeCallback), state);
                }
            }
            catch (Exception)
            {
                clientList.Remove(client);
                Console.WriteLine("Client Disconnected.");
            }
        }

        private static void Send(Socket client, string data)
        {
            byte[] fixedByteArray = BitConverter.GetBytes(data.Length);
            List<byte> listOfData = new List<byte>();

            byte[] byteData = Encoding.UTF8.GetBytes(data);
            listOfData.AddRange(fixedByteArray);
            listOfData.AddRange(byteData);
            var dataToSend = listOfData.ToArray();

            client.BeginSend(dataToSend, 0, dataToSend.Length, 0, new AsyncCallback(SendCallback), client);
            Broadcast(client, dataToSend);
        }

        private static void SendCallback(IAsyncResult ar)
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

        private static void Broadcast (Socket currentClient, byte[] messageData)
        {
            foreach (var client in clientList)
            {
                if (client != currentClient)
                    client.BeginSend(messageData, 0, messageData.Length, 0, new AsyncCallback(BroadcastCallback), client);
            }
        }

        private static void BroadcastCallback(IAsyncResult ar)
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
