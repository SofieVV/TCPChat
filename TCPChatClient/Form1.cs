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
            {
                SendChosenClient(client);
                SendMessage();
            }

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

                client.Receive(state.command, StateObject.enumCommand, SocketFlags.None);
                ExecuteCommand(state);
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public void ExecuteCommand(StateObject state)
        {
            try
            {
                Command command = (Command)BitConverter.ToInt32(state.command, 0);

                switch (command)
                {
                    case Command.Add:
                        {
                            string[] names = ReceiveClientListNames(state);
                            foreach (var name in names)
                            {
                                if (!name.Equals(clientNameTextBox.Text))
                                    ClientListBox.Items.Add(name);
                            }

                            break;
                        }
                    case Command.Remove:
                        {
                            ClientListBox.Items.Remove(GetClientName(state));
                            ReceiveMessage(state);
                            break;
                        }
                    case Command.Message:
                        {
                            string name = GetClientName(state);
                            PrintMessage(state, name);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public string ReceiveMessage(StateObject state)
        {
            try
            {
                state.client.Socket.Receive(receivedMessageData, receivedMessageSize, SocketFlags.None);
                string message = string.Empty; 

                if (receivedMessageSize > 0)
                {
                    state.stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, receivedMessageSize));
                    state.client.Socket.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(RecieveMessageSizeCallback), state);
                    message = state.stringBuilder.ToString();
                    state.stringBuilder.Clear();
                }

                return message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string[] ReceiveClientListNames(StateObject state)
        {
            string connectedClientNames = ReceiveMessage(state);
            return connectedClientNames.Split(',');
        }

        public string GetClientName(StateObject state)
        {
            try
            {
                state.client.Socket.Receive(state.client.clientNameBuffer, Client.nameSize, SocketFlags.None);
                state.stringBuilder.Append(Encoding.UTF8.GetString(state.client.clientNameBuffer, 0, Client.nameSize));
                newClient = state.stringBuilder.ToString().TrimEnd('\0');

                state.stringBuilder.Clear();
                return newClient;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public void PrintMessage (StateObject state, string clientName)
        {
            try
            {
                response = ReceiveMessage(state);
                ChatWriteLine(clientName + response);
            }
            catch (Exception e)
            {
                //ChatWriteLine(e.Message);
            }
        }

        public void SendChosenClient(Socket client)
        {
            byte[] name;

            if (ClientListBox.SelectedItem != null)
                name = Encoding.UTF8.GetBytes(ClientListBox.GetItemText(ClientListBox.SelectedItem.ToString()));
            else
                name = Encoding.UTF8.GetBytes(clientNameTextBox.Text);

            client.BeginSend(name, 0, name.Length, 0, new AsyncCallback(SendCallback), client);
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
