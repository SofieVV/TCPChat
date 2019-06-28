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
        private string response = string.Empty;
        private static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
        private Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        private StringBuilder stringBuilder = new StringBuilder();
        private int receivedMessageSize = 0;
        private byte[] receivedMessageData = null;
        private string newClient = string.Empty;

        public TCPClient()
        {
            InitializeComponent();
        }

        private void TCPClient_Load(object sender, EventArgs e)
        {
            StartClient();
        }
        private void LoginButton_Click(object sender, EventArgs e)
        {
            var state = new StateObject();

            if (clientNameTextBox.Text.Length <= 0)
                MessageBox.Show("Please eneter an username.", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else if (clientNameTextBox.Text.Contains(" "))
            {
                MessageBox.Show("Username can not contain empty spaces!", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                clientNameTextBox.Clear();
            }
            else if (Encoding.UTF8.GetByteCount(clientNameTextBox.Text) > Client.NAME_SIZE)
            {
                MessageBox.Show("Username is too long!", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                clientNameTextBox.Clear();
            }
            else
            {
                SendName(client, clientNameTextBox.Text);
                client.Receive(state.Command, StateObject.COMMAND_SIZE, SocketFlags.None);
                Command command = (Command)BitConverter.ToInt32(state.Command, 0);

                switch (command)
                {
                    case Command.Success:
                        clientNameTextBox.ReadOnly = true;
                        LoginButton.Enabled = false;
                        Receive(client);
                        break;
                    case Command.Error:
                        MessageBox.Show("Username is already taken!", "Invalid username!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        clientNameTextBox.Clear();
                        break;
                }
            }
        }

        public void SendName(Socket client, string name)
        {
            byte[] nameData = Encoding.UTF8.GetBytes(name);
            client.BeginSend(nameData, 0, nameData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (ClientListBox.SelectedIndex == -1)
                MessageBox.Show("Please choose a client to talk to.", "Invalid request!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
                SendMessage();

            MessageTextBox.Clear();
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
            }
            catch (Exception)
            {
                ChatWriteLine("Server is down. Please try again later.");
            }
        }

        public void SendMessage()
        {
            try
            {
                SendChosenClient(client);
                Send(client, MessageTextBox.Text);
            }
            catch (Exception)
            {
                ChatWriteLine("A problem occurred while sending your message.");
            }
        }

        public string GetChosenNameFromClientList()
        {
            string name = string.Empty;

            if (ClientListBox.SelectedIndex != -1)
                name = ClientListBox.SelectedItem.ToString();

            return name;
        }

        public void SendChosenClient(Socket client)
        {
            string name = GetChosenNameFromClientList();
            byte[] chosenName = Encoding.UTF8.GetBytes(name);
            client.BeginSend(chosenName, 0, chosenName.Length, 0, new AsyncCallback(SendCallback), client);

        }

        public void Send(Socket client, string data)
        {
            var listOfData = new List<byte>();

            int dataByteLength = Encoding.UTF8.GetByteCount(data);
            listOfData.AddRange(BitConverter.GetBytes(dataByteLength));
            listOfData.AddRange(Encoding.UTF8.GetBytes(data));

            var dataToSend = listOfData.ToArray();
            client.BeginSend(dataToSend, 0, dataToSend.Length, 0, new AsyncCallback(SendCallback), client);
        }

        public void SendCallback(IAsyncResult ar)
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

        public void Receive(Socket client)
        {
            try
            {
                var state = new StateObject();
                state.Client.Socket = client;
                client.BeginReceive(state.Buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(RecieveMessageSizeCallback), state);
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public void RecieveMessageSizeCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var clientSocket = state.Client.Socket;

            try
            {
                receivedMessageSize = BitConverter.ToInt32(state.Buffer, 0);
                receivedMessageData = new byte[receivedMessageSize];

                clientSocket.Receive(state.Command, StateObject.COMMAND_SIZE, SocketFlags.None);
                ExecuteCommand(state);
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public void ExecuteCommand(StateObject state)
        {
            try
            {
                Command command = (Command)BitConverter.ToInt32(state.Command, 0);

                switch (command)
                {
                    case Command.Add:
                        string[] names = ReceiveClientListNames(state);
                        foreach (var clientName in names)
                            if (!clientName.Equals(clientNameTextBox.Text))
                                ClientListBox.Items.Add(clientName);

                        break;
                    case Command.Remove:
                        ClientListBox.Items.Remove(GetClientName(state));
                        ReceiveMessage(state);
                        break;
                    case Command.Message:
                        string name = GetClientName(state);
                        PrintMessage(state, name);
                        break;
                }
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        public string ReceiveMessage(StateObject state)
        {
            try
            {
                state.Client.Socket.Receive(receivedMessageData, receivedMessageSize, SocketFlags.None);
                string message = string.Empty;

                if (receivedMessageSize > 0)
                {
                    stringBuilder.Append(Encoding.UTF8.GetString(receivedMessageData, 0, receivedMessageSize));
                    state.Client.Socket.BeginReceive(state.Buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(RecieveMessageSizeCallback), state);
                    message = stringBuilder.ToString();
                    stringBuilder.Clear();
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
                state.Client.Socket.Receive(state.Client.ClientNameBuffer, Client.NAME_SIZE, SocketFlags.None);
                stringBuilder.Append(Encoding.UTF8.GetString(state.Client.ClientNameBuffer, 0, Client.NAME_SIZE));
                newClient = stringBuilder.ToString().TrimEnd('\0');

                stringBuilder.Clear();
                return newClient;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public void PrintMessage(StateObject state, string clientName)
        {
            try
            {
                response = ReceiveMessage(state);

                if (clientName == clientNameTextBox.Text)
                {
                    ChatTextBox.SelectionColor = Color.Crimson;
                    ChatWriteLine($"TO {ClientListBox.SelectedItem.ToString()}: {response}");
                }
                else
                {
                    if (clientName == GetChosenNameFromClientList())
                        ChatTextBox.SelectionColor = Color.Blue;

                    ChatWriteLine($"{clientName}: {response}");
                }
            }
            catch (Exception e)
            {
                ChatWriteLine(e.Message);
            }
        }

        private void ChatWriteLine(string message)
        {
            ChatTextBox.AppendText(message + Environment.NewLine);
        }
    }
}