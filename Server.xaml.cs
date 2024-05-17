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
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Socket soket;
        private List<Socket> clients = new List<Socket>();
        private List<string> userNames = new List<string>();
        private Dictionary<string, string> clientIpToName = new Dictionary<string, string>();

        public Server()
        {
            InitializeComponent();
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Bind(ipPoint);
            soket.Listen(1000);

            Task.Run(() => ListenToClients(cancellationTokenSource.Token), cancellationTokenSource.Token);

        }

        private async Task ListenToClients(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                var client = await soket.AcceptAsync();
                clients.Add(client);
                string userNamesString = string.Join("#", userNames);
                await SendName(client, userNamesString);

                Task.Run(() => RecieveMessage(client, cancellationToken), cancellationToken);
            
            }
        }

        private async Task RecieveMessage(Socket client, CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                byte[] byritos = new byte[1024];
                int BytesRecirverd = await client.ReceiveAsync(byritos);
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

                    //cловарик для дисконекта ip зачение name ключ
                    IPEndPoint remoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
                    string clientIp = remoteEndPoint.Address.ToString();
                    clientIpToName[clientIp] = name;
                }
                if (message.Contains("/disconnect"))
                {
                    DisconnectClient(client);
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

        private async Task DisconnectClient(Socket client)
        {
            
            IPEndPoint remoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
            string clientIp = remoteEndPoint.Address.ToString();

            // из сокета
            clients.Remove(client);

            // Удаление записи из словаря
            if (clientIpToName.ContainsKey(clientIp))
            {
                string userName = clientIpToName[clientIp];
                clientIpToName.Remove(clientIp);

                // в именах
                userNames.Remove(userName);
                UsersLbx.Items.Remove(userName);

                
                string message = $"{userName} отключился.";
                MessagesLbx.Items.Add(message);
                foreach (var item in clients)
                {
                    await SendMessage(item, message);
                }
            }

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        public void CloseServer()
        {
            cancellationTokenSource.Cancel();
            soket.Close();
        }
    }
}
    