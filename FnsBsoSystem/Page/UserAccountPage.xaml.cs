using FnsBsoSystem.Entities;
using FnsBsoSystem.Window;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для UserAccountPage.xaml
    /// </summary>
    public partial class UserAccountPage
    {
        public UserAccountPage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                // Используй СВОЕ имя подключения
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // Ищем пользователя по ID, который сохранили при входе
                    var user = db.Sys_Users.Find(App.CurrentUserId);

                    if (user != null)
                    {
                        // Данные учетной записи
                        LblLogin.Text = user.Login;
                        LblRole.Text = user.Role; // Показываем роль под фото

                        // Данные сотрудника (через связь)
                        if (user.Main_Employees != null)
                        {
                            var emp = user.Main_Employees;

                            // Полное имя для правой части
                            LblFullName.Text = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}";

                            // Короткое имя для левой части (под фото)
                            LblShortName.Text = $"{emp.LastName} {emp.FirstName.Substring(0, 1)}.";

                            // Безопасное получение отдела и должности (?. - проверка на null)
                            LblDept.Text = emp.Ref_Departments?.DeptName ?? "Не указан";
                            LblPos.Text = emp.Ref_Positions?.PosName ?? "Не указана";

                            // Загрузка фото
                            if (emp.Photo != null && emp.Photo.Length > 0)
                            {
                                ImgProfile.ImageSource = BytesToImage(emp.Photo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки профиля: " + ex.Message);
            }
        }

        // СМЕНА ФОТО
        private void BtnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Картинки|*.jpg;*.png;*.jpeg;*.bmp";

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(ofd.FileName);

                    using (var db = new IFNS6_BsoSystemEntities())
                    {
                        var user = db.Sys_Users.Find(App.CurrentUserId);
                        // Проверяем, привязан ли сотрудник
                        if (user != null && user.Main_Employees != null)
                        {
                            user.Main_Employees.Photo = imageBytes;
                            db.SaveChanges();

                            // Обновляем картинку на экране сразу
                            ImgProfile.ImageSource = BytesToImage(imageBytes);
                            MessageBox.Show("Фотография успешно обновлена!");
                        }
                        else
                        {
                            MessageBox.Show("К этому пользователю не привязан сотрудник в базе данных.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения фото: " + ex.Message);
                }
            }
        }

        // СМЕНА ПАРОЛЯ
        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtNewPass.Text))
                {
                    MessageBox.Show("Введите новый пароль!");
                    return;
                }

                using (var db = new IFNS6_BsoSystemEntities())
                {
                    var user = db.Sys_Users.Find(App.CurrentUserId);
                    if (user != null)
                    {
                        user.Password = TxtNewPass.Text;
                        db.SaveChanges();
                        MessageBox.Show("Пароль успешно изменен!");
                        TxtNewPass.Text = ""; // Очистить поле
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка смены пароля: " + ex.Message);
            }
        }

        // МЕТОД КОНВЕРТАЦИИ (Байты -> Картинка)
        private BitmapImage BytesToImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze(); // Важно для потоков WPF
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {

            // Открываем окно редактирования
            EditProfileWindow win = new EditProfileWindow();

            // Если нажали "Сохранить" (DialogResult == true)
            if (win.ShowDialog() == true)
            {
                // Перезагружаем данные на странице, чтобы увидеть изменения
                LoadUserData();
            }
        }
    }
}