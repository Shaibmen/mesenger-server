using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Server_app
{
    /// <summary>
    /// Логика взаимодействия для Server.xaml
    /// </summary>
    public partial class Server : Window
    {
        private Socket soket;
        private List<Socket> clients = new List<Socket>();
        private List<string> userNames = new List<string>();
        public Server()
        {
            InitializeComponent();
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Bind(ipPoint);
            soket.Listen(1000);

            ListenToClients();

            
        }

        private async Task ListenToClients()
        {
            while(true)
            {
                var client = await soket.AcceptAsync();
                clients.Add(client);

                string userNamesString = string.Join("#", userNames);
                await SendName(client, userNamesString);

                RecieveMessage(client);
            }
        }

        private async Task RecieveMessage(Socket client)
        {
            while(true)
            {
                byte[] byritos = new byte[1024];
                await client.ReceiveAsync(byritos);
                string message = Encoding.UTF8.GetString(byritos);

                if (message.Contains("#"))
                {
                    string name = message.Split('#')[1];

                    if (!userNames.Contains(name))
                    {
                        userNames.Add(name);
                    }

                    UsersLbx.Items.Add(name);
                    foreach (var item in clients)
                    {
                        SendName(item, $"#{name}");
                    }
                }
                else
                {
                    MessagesLbx.Items.Add(message);
                    foreach (var item in clients)
                    {
                        SendMessage(item, message);
                    }
                }
            }
        }

        private async Task SendMessage(Socket client, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }
        
        private async Task SendName(Socket client, string name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            await client.SendAsync(bytes, SocketFlags.None);
        }
    }
}
