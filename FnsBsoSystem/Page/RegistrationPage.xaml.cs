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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FnsBsoSystem.Page
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage
    {
        public RegistrationPage()
        {
            InitializeComponent();
            LoadDictionaries();
        }

        private void LoadDictionaries()
        {
            using (var db = new IFNS6_BsoSystemEntities())
            {
                ComboDept.ItemsSource = db.Ref_Departments.ToList();
                ComboPos.ItemsSource = db.Ref_Positions.ToList();
                ComboRank.ItemsSource = db.Ref_Ranks.ToList();
            }
        }

        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- СБОР ОШИБОК ---
                StringBuilder errors = new StringBuilder();

                // 1. Личные данные
                if (string.IsNullOrWhiteSpace(TxtLast.Text))
                    errors.AppendLine("- Не указана Фамилия");

                if (string.IsNullOrWhiteSpace(TxtFirst.Text))
                    errors.AppendLine("- Не указано Имя");

                if (ComboDept.SelectedValue == null)
                    errors.AppendLine("- Не выбран Отдел");

                if (ComboPos.SelectedValue == null)
                    errors.AppendLine("- Не выбрана Должность");

                if (ComboRank.SelectedValue == null)
                    errors.AppendLine("- Не выбран Классный чин");

                // 2. Данные аккаунта
                if (string.IsNullOrWhiteSpace(TxtLogin.Text))
                    errors.AppendLine("- Не указан Логин");

                if (string.IsNullOrWhiteSpace(TxtPass.Text))
                    errors.AppendLine("- Не указан Пароль");

                if (ComboRole.SelectedItem == null)
                    errors.AppendLine("- Не выбрана Роль доступа");

                // Если есть ошибки - показываем и выходим
                if (errors.Length > 0)
                {
                    MessageBox.Show("Для регистрации заполните следующие поля:\n\n" + errors.ToString(),
                                    "Неполные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- СОХРАНЕНИЕ ---
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // Проверка на дубликат логина
                    if (db.Sys_Users.Any(u => u.Login == TxtLogin.Text))
                    {
                        MessageBox.Show($"Логин '{TxtLogin.Text}' уже занят! Придумайте другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 1. Создаем Сотрудника
                    var newEmp = new Main_Employees
                    {
                        LastName = TxtLast.Text,
                        FirstName = TxtFirst.Text,
                        MiddleName = TxtMiddle.Text,
                        DeptId = (int)ComboDept.SelectedValue,
                        PosId = (int)ComboPos.SelectedValue,
                        RankId = (int)ComboRank.SelectedValue,
                        IsActive = true
                    };
                    db.Main_Employees.Add(newEmp);
                    db.SaveChanges(); // Получаем ID

                    // 2. Создаем Пользователя
                    var newUser = new Sys_Users
                    {
                        EmployeeId = newEmp.Id,
                        Login = TxtLogin.Text,
                        Password = TxtPass.Text,
                        Role = (ComboRole.SelectedItem as ComboBoxItem).Content.ToString()
                    };
                    db.Sys_Users.Add(newUser);
                    db.SaveChanges();

                    // 3. Запись в журнал
                    var log = new Log_Operations
                    {
                        UserId = App.CurrentUserId,
                        ActionType = "Регистрация",
                        Details = $"Создан новый пользователь: {newUser.Login} (Сотр: {newEmp.LastName})",
                        OperationDate = DateTime.Now
                    };
                    db.Log_Operations.Add(log);
                    db.SaveChanges();
                }

                MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка полей
                TxtLast.Clear(); TxtFirst.Clear(); TxtMiddle.Clear();
                TxtLogin.Clear(); TxtPass.Clear();
                ComboDept.SelectedIndex = -1; ComboPos.SelectedIndex = -1; ComboRank.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }
    }
}