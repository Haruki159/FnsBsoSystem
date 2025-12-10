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

namespace FnsBsoSystem.Window
{
    /// <summary>
    /// Логика взаимодействия для EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow 
    {
        public EditProfileWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // 1. Загружаем списки для ComboBox
                    ComboDept.ItemsSource = db.Ref_Departments.ToList();
                    ComboPos.ItemsSource = db.Ref_Positions.ToList();

                    // 2. Находим текущего сотрудника через пользователя
                    var user = db.Sys_Users.Find(App.CurrentUserId);
                    if (user != null && user.Main_Employees != null)
                    {
                        var emp = user.Main_Employees;

                        // Заполняем поля текущими данными
                        TxtLast.Text = emp.LastName;
                        TxtFirst.Text = emp.FirstName;
                        TxtMiddle.Text = emp.MiddleName;

                        ComboDept.SelectedValue = emp.DeptId;
                        ComboPos.SelectedValue = emp.PosId;
                    }
                }
            }
            catch { }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    var user = db.Sys_Users.Find(App.CurrentUserId);
                    if (user != null && user.Main_Employees != null)
                    {
                        // Обновляем данные
                        user.Main_Employees.LastName = TxtLast.Text;
                        user.Main_Employees.FirstName = TxtFirst.Text;
                        user.Main_Employees.MiddleName = TxtMiddle.Text;

                        if (ComboDept.SelectedValue != null)
                            user.Main_Employees.DeptId = (int)ComboDept.SelectedValue;

                        if (ComboPos.SelectedValue != null)
                            user.Main_Employees.PosId = (int)ComboPos.SelectedValue;

                        db.SaveChanges();

                        MessageBox.Show("Данные успешно обновлены!");
                        DialogResult = true; // Закрываем окно с успехом
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
