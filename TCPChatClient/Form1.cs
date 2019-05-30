using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPChat.Common;

namespace TCPChatClient
{
    public partial class TCPClient : Form
    {
        private const int port = 1020;
        private const int fixedSize = 4;
        private static string response = string.Empty;
        private static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private static IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
        private static Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        private static int receivedMessageSize = 0;
        private static byte[] receivedMessageData = null;


        public TCPClient()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            StartClient();
            Receive(client);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
            MessageTextBox.Clear();
        }

        public void SendMessage()
        {
            try
            {
                Send(client, MessageTextBox.Text);
            }
            catch(Exception)
            {
                ChatWriteLine("Please connect first.");
            }
        }

        public void StartClient()
        {
            try
            {
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            }
            catch (Exception)
            {
                ChatWriteLine("You are already connected.");
            }
        }

        public void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                ChatWriteLine($"Socket connected to {client.RemoteEndPoint.ToString()}");
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }


        public void Receive (Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(RecieveSizeCallback), state);
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public void RecieveSizeCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            try
            {
                receivedMessageSize = BitConverter.ToInt32(state.buffer, 0);
                receivedMessageData = new byte[receivedMessageSize];
                client.BeginReceive(receivedMessageData, 0, receivedMessageSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public void ReceiveCallback (IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, bytesRead));
                    client.BeginReceive(receivedMessageData, 0, receivedMessageSize, 0, new AsyncCallback(ReceiveCallback), state);
                    response = state.stringBuilder.ToString();
                    state.stringBuilder.Clear();

                    ChatWriteLine(response);
                }
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public void Send(Socket client, string data)
        {
            byte[] size = BitConverter.GetBytes(data.Length);
            byte[] fixedByteArray = new byte[fixedSize];
            List<byte> listOfData = new List<byte>();

            for (int i = 0; i < fixedSize; i++)
            {
                if (size.Length > i)
                    fixedByteArray[i] = size[i];
                else
                    fixedByteArray[i] = 0;
            }

            byte[] byteData = Encoding.UTF8.GetBytes(data);
            listOfData.AddRange(fixedByteArray);
            listOfData.AddRange(byteData);

            var dataToSend = listOfData.ToArray();
            client.BeginSend(dataToSend, 0, dataToSend.Length, 0, new AsyncCallback(SendCallback), client);
        }

        public void SendCallback (IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        private void ChatWriteLine(string message)
        {
            ChatTextBox.Text += message + Environment.NewLine;
        }
    }
}
