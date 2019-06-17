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
        private static List<string> connectedClients = new List<string>();
        private static string newClient = string.Empty;

        public TCPClient()
        {
            InitializeComponent();
        }
        private void ConnectButton_Click(object sender, EventArgs e)
        {

            if (clientNameTextBox.Text.Length > Client.nameSize || clientNameTextBox.Text.Length <= 0)
            {
                MessageBox.Show("Username must be shorter that 10 characters!", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clientNameTextBox.Clear();
            }
            else if (ClientListBox.Items.Contains(clientNameTextBox.Text))
            {
                MessageBox.Show("Username is already taken!", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clientNameTextBox.Clear();
            }
            else
            {
                clientNameTextBox.ReadOnly = true;
                ConnectButton.Enabled = false;
                StartClient();
                SendName(client, clientNameTextBox.Text);
                Receive(client);
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (MessageTextBox.Text != string.Empty)
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
                //ChatWriteLine("Please connect first.");
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
                //ChatWriteLine("You are already connected.");
            }
        }

        public void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
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
                //ChatWriteLine(e.Message);
            }
        }


        public void Receive (Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.client.Socket = client;
                client.BeginReceive(state.client.clientNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(GetClientNameCallback), state);
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public void GetClientNameCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;

            try
            {
                int bytesRead = client.EndReceive(ar);

                state.stringBuilder.Append(Encoding.UTF8.GetString(state.client.clientNameBuffer, 0, bytesRead));
                newClient = state.stringBuilder.ToString().TrimEnd('\0');

                client.BeginReceive(state.command, 0, StateObject.enumCommand, 0, new AsyncCallback(UpdateClientListCallback), state);
                state.stringBuilder.Clear();
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public void UpdateClientListCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.client.Socket;

            try
            {
                Command command = (Command)BitConverter.ToInt32(state.command, 0);

                switch (command)
                {
                    case Command.Add:
                        {
                            if (!ClientListBox.Items.Contains(newClient))
                                ClientListBox.Items.Add(newClient);
                            break;
                        }
                    case Command.Remove:
                        {
                            ClientListBox.Items.Remove(newClient);
                            break;
                        }
                    case Command.Updated:
                            break;
                }

                client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(RecieveMessageSizeCallback), state);
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public void RecieveMessageSizeCallback(IAsyncResult ar)
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
                //ChatWriteLine(e.Message);
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
                    client.BeginReceive(state.client.clientNameBuffer, 0, Client.nameSize, 0, new AsyncCallback(GetClientNameCallback), state);
                    response = state.stringBuilder.ToString();
                    state.stringBuilder.Clear();

                    ChatWriteLine(newClient + response);
                }
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public void Send(Socket client, string data)
        {
            List<byte> listOfData = new List<byte>();

            listOfData.AddRange(BitConverter.GetBytes(data.Length));
            listOfData.AddRange(Encoding.UTF8.GetBytes(data));

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
                //ChatWriteLine(e.Message);
            }
        }

        private void ChatWriteLine(string message)
        {
            ChatTextBox.Text += message + Environment.NewLine;
        }
    }
}
