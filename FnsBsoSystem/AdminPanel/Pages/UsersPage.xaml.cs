using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FnsBsoSystem.Entities;

namespace FnsBsoSystem.AdminPanel.Pages
{
    public partial class UsersPage
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new IFNS6_BsoSystemEntities())
            {
                // Выбираем пользователей вместе с их данными сотрудника
                dgUsers.ItemsSource = db.Sys_Users.Select(u => new {
                    u.Id,
                    FullName = u.Main_Employees.FullName,
                    u.Login,
                    u.Main_Employees.IsActive
                }).ToList();
            }
        }

        private void BtnToggleBlock_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgUsers.SelectedItem as dynamic;
            if (selected == null) return;

            int userId = (int)selected.Id;

            using (var db = new IFNS6_BsoSystemEntities())
            {
                // Ищем сотрудника, привязанного к пользователю
                var user = db.Sys_Users.FirstOrDefault(u => u.Id == userId);
                if (user != null && user.Main_Employees != null)
                {
                    // Инвертируем статус
                    user.Main_Employees.IsActive = !user.Main_Employees.IsActive;

                    db.SaveChanges();
                    LoadData(); // Обновляем таблицу
                    MessageBox.Show("Статус пользователя изменен!");
                }
            }
        }
    }
}