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

            if (clientNameTextBox.Text.Length > StateObject.nameSize)
            {
                MessageBox.Show("Username must be shorter that 20 characters!", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clientNameTextBox.Clear();
            }
            else
            {
                clientNameTextBox.ReadOnly = true;
                StartClient();
                SendName(client, clientNameTextBox.Text);
                Receive(client);
            }
        }

        public BindingList<string> Names { get; set; }

        public void RemoveName()
        {
            var pesho = Names.FirstOrDefault(n => n == "Pesho");
            Names.Remove(pesho);
        }

        public void AddName()
        {
            Names.Add("Pesho");
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

        public void SendName(Socket client, string name)
        {
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            client.BeginSend(nameData, 0, nameData.Length, 0, new AsyncCallback(SendNameCallback), client);
        }

        public void SendNameCallback(IAsyncResult ar)
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


        public void Receive (Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.client.Socket = client;
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
            Socket client = state.client.Socket;

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
                Socket client = state.client.Socket;
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, bytesRead));
                    client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(RecieveSizeCallback), state);
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
            byte[] fixedByteArray = BitConverter.GetBytes(data.Length);
            List<byte> listOfData = new List<byte>();

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
