using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPClientChat
{
    class ClientProgram
    {
        private const int port = 1020;
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static string response = string.Empty;

        static void Main(string[] args)
        {
            Console.Write("Enter server name: ");
            string serverName = Console.ReadLine();
            StartClient(serverName);    
        }

        public static void StartClient(string serverName)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(serverName);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                string message = Console.ReadLine();
                Send(client, message);
                sendDone.WaitOne();

                Receive(client);
                receiveDone.WaitOne();

                Console.WriteLine(response);

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnected.");
            }
        }

        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine($"Socket connected to {client.RemoteEndPoint.ToString()}{Environment.NewLine}");
                connectDone.Set();
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnected.");
            }
        }

        public static void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnected.");
            }
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    if (state.sb.Length > 1)
                        response = state.sb.ToString();

                    receiveDone.Set();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnected.");
            }
        }

        public static void Send(Socket client, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);

                sendDone.Set();
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnected.");
            }
        }
    }
}
