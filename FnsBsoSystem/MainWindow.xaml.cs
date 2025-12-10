using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FnsBsoSystem.Class;
using FnsBsoSystem.Entities;
using FnsBsoSystem.Page;

namespace FnsBsoSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow() 
        {
            InitializeComponent();
            // 1. Проверяем права доступа
            CheckAccess();
            Manager.MainFrame = MainFrame;
            Manager.MainFrame.Navigate(new StockPage()); 
        }
        private void NavStock(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Visible;
            // ПОТОМ переходим
            Manager.MainFrame.Navigate(new StockPage());
        }
        private void NavHistory(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Visible;
            Manager.MainFrame.Navigate(new HistoryPage());
        }
        private void BtnExit(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuthWindow loginWindow = new AuthWindow();
                loginWindow.Show();
                this.Close();
            }
        }
        private void NavRegistration(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Visible;
            Manager.MainFrame.Navigate(new RegistrationPage());
        }

        private void NavAccount(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Visible;
            Manager.MainFrame.Navigate(new UserAccountPage());
        }

        // ПРОВЕРКА ПРАВ
        private void CheckAccess()
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // Проверяем, задан ли ID (чтобы не упало, если запускаем без авторизации при тестах)
                    if (App.CurrentUserId == 0) return;

                    // Получаем текущего пользователя из базы
                    var user = db.Sys_Users.Find(App.CurrentUserId);

                    if (user != null)
                    {
                        // Логика: Доступ к регистрации имеют только Admin и Viewer (Босс)
                        // Кладовщик (Storekeeper) кнопку не увидит.
                        if (user.Role == "Admin" || user.Role == "Viewer")
                        {
                            BtnNavReg.Visibility = Visibility.Visible; // Кнопка есть в XAML с x:Name="BtnNavReg"
                        }
                        else
                        {
                            BtnNavReg.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при проверке прав доступа: " + ex.Message);
            }
        }

        private void NavHome(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Hidden;
            // Очищаем историю переходов, чтобы кнопка "Назад" не вела на страницы
            while (Manager.MainFrame.CanGoBack)
            {
                Manager.MainFrame.RemoveBackEntry();
            }
        }
    }
}