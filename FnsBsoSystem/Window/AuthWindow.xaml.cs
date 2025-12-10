using FnsBsoSystem.AdminPanel;
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
                    var user = db.Sys_Users.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == TxtPass.Password);

                    if (user != null)
                    {
                        // ПРОВЕРКА НА БЛОКИРОВКУ (по полю IsActive из Main_Employees)
                        // Если Main_Employees не null и IsActive == false, значит заблокирован
                        if (user.Main_Employees != null && user.Main_Employees.IsActive == false)
                        {
                            MessageBox.Show("Ваша учетная запись заблокирована! Обратитесь к администратору.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // 2. Запоминаем ID
                        App.CurrentUserId = user.Id;

                        // 3. Пишем в журнал
                        var loginLog = new Log_Operations
                        {
                            UserId = user.Id,
                            ActionType = "Вход в систему",
                            Details = $"Авторизация пользователя {user.Login}",
                            OperationDate = DateTime.Now
                        };
                        db.Log_Operations.Add(loginLog);
                        db.SaveChanges();

                        // 4. ПРОВЕРКА РОЛИ И ПЕРЕНАПРАВЛЕНИЕ
                        // Допустим, у тебя в Sys_Roles роль с именем "Admin" имеет Id = 1
                        // Проверь в базе, какой ID у Админа!
                        if (user.Role == 1) // Замени 1 на реальный ID админа из твоей таблицы Sys_Roles
                        {
                            MessageBox.Show("Добро пожаловать в Админ-панель!", "Администратор", MessageBoxButton.OK, MessageBoxImage.Information);
                            AdminMainWindow adminWindow = new AdminMainWindow(user);
                            adminWindow.Show();
                        }
                        else
                        {
                            MessageBox.Show($"Добро пожаловать, {user.Login}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            MainWindow main = new MainWindow(user); // Используем наш новый конструктор
                            main.Show();
                        }

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