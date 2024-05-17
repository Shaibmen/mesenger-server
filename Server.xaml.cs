using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        private Dictionary<string, string> clientIpToName = new Dictionary<string, string>();

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
            while (true)
            {
                var client = await soket.AcceptAsync();
                clients.Add(client);

                RecieveMessage(client);
            }
        }

        private async Task HandleClientDisconnect(Socket client)
        {
            try
            {
                await client.ReceiveAsync(new ArraySegment<byte>(new byte[0]), SocketFlags.None);
            }
            catch (SocketException)
            {
                // соединение было прервано 
            }
            DisconnectClient(client);
            client.Close();
        }

        private async Task RecieveMessage(Socket client)
        {
            try
            {
                while (true)
                {
                    byte[] byritos = new byte[1024];
                    int byresReceived = await client.ReceiveAsync(byritos);
                    if (byresReceived == 0)
                    {
                        await HandleClientDisconnect(client);
                        break;
                    }
                    string message = Encoding.UTF8.GetString(byritos);

                    if (message.Contains("#"))
                    {
                        string name = message.Split('#')[1];

                        if (!userNames.Contains(name))
                        {
                            userNames.Add(name);
                            UsersLbx.Items.Add(name);

                            // отправляем список всех пользователей новому участнику
                            foreach (var item in clients)
                            {
                                if (item == client) // cокет нового участника
                                {
                                    SendName(item, "#" + string.Join("#", userNames));
                                }
                            }

                            //для старых участников #не забыть
                            string newUserMessage = $"#{name}";
                            MessagesLbx.Items.Add(newUserMessage);
                            foreach (var item in clients)
                            {
                                if (item != client)
                                {
                                    SendMessage(item, newUserMessage);
                                }
                            }
                        }

                        //cловарик для дисконекта ip зачение name ключ
                        IPEndPoint remoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
                        string clientIp = remoteEndPoint.Address.ToString();
                        clientIpToName[clientIp] = name;
                    }

                    else if (message.Contains("/disconnect"))
                    {
                        // заменяем вызов DisconnectClient(client) на HandleClientDisconnect(client) 
                        await HandleClientDisconnect(client);
                        break;
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
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
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
    