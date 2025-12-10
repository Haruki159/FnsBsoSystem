using FnsBsoSystem.AdminPanel.Pages;
using FnsBsoSystem.Entities;
using FnsBsoSystem.Page;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace FnsBsoSystem.AdminPanel
{
    /// <summary>
    /// Логика взаимодействия для AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow
    {
        private Sys_Users _currentAdmin;

        public AdminMainWindow(Sys_Users admin)
        {
            InitializeComponent();
            _currentAdmin = admin;
            // По умолчанию грузим пользователей
            AdminFrame.Navigate(new UsersPage());
        }

        private void NavUsers_Click(object sender, RoutedEventArgs e) => AdminFrame.Navigate(new UsersPage());
        private void NavDicts_Click(object sender, RoutedEventArgs e) => AdminFrame.Navigate(new DictsPage());
        private void NavLogs_Click(object sender, RoutedEventArgs e) => AdminFrame.Navigate(new HistoryPage());

        private void BtnSwitchToUser_Click(object sender, RoutedEventArgs e)
        {
            MainWindow userWin = new MainWindow(_currentAdmin);
            userWin.Show();
            this.Close();
        }
    }
}