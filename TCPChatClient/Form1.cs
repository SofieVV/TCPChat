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
        private static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private static IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
        private static Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


        public TCPClient()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            StartClient();
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
                Receive(client);
            }
            catch(Exception e)
            {
                ChatWriteLine(e.Message);
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
                client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReceiveCallback), state);
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
                string response = "";
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    response = state.stringBuilder.ToString();
                    state.stringBuilder.Clear();
                }

                ChatWriteLine(response);
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public void Send(Socket client, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        public void SendCallback (IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
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
