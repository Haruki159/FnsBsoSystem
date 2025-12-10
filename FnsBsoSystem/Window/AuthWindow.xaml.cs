using FnsBsoSystem.Entities;
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

namespace FnsBsoSystem
{
    public partial class AuthWindow 
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // 1. Проверяем логин и пароль
                    // Важно: У PasswordBox свойство .Password, а не .Text
                    var user = db.Sys_Users.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == TxtPass.Password);

                    if (user != null)
                    {
                        // 2. Запоминаем ID
                        App.CurrentUserId = user.Id;

                        // 3. Пишем в журнал, что пользователь ВОШЕЛ
                        var loginLog = new Log_Operations
                        {
                            UserId = user.Id,
                            ActionType = "Вход в систему",
                            Details = $"Авторизация пользователя {user.Login}",
                            OperationDate = DateTime.Now
                        };
                        db.Log_Operations.Add(loginLog);
                        db.SaveChanges();

                        // 4. Показываем сообщение и открываем программу
                        MessageBox.Show($"Добро пожаловать, {user.Login}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        MainWindow main = new MainWindow();
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}