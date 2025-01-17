﻿using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameUser.Text))
            {
                string name = NameUser.Text;

                var Server1 = new Server(name);
                Server1.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Заполните имя (опционально)");
            }
            
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameUser.Text) && !string.IsNullOrEmpty(IpAddres.Text))
            {
                string ip = IpAddres.Text;
                string name = NameUser.Text;
                ClientWindow clientWindow = new ClientWindow(ip, name);
                clientWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("заполни все поля");
            }
        }
    }
}