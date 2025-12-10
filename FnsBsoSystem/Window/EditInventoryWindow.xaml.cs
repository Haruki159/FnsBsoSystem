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
    public partial class EditInventoryWindow
    {
        private int _idToEdit;
        private bool _isLoaded = false;

        public EditInventoryWindow(int id)
        {
            InitializeComponent();
            _idToEdit = id;
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new IFNS6_BsoSystemEntities())
            {
                // Загружаем списки для ComboBox
                ComboStatus.ItemsSource = db.Ref_BlankStatuses.ToList();
                ComboEmp.ItemsSource = db.Main_Employees.Select(e => new {
                    Id = e.Id,
                    FullName = e.LastName + " " + e.FirstName + " " + e.MiddleName
                }).ToList();

                // Загружаем саму запись
                var item = db.Main_Inventory.Find(_idToEdit);
                if (item != null)
                {
                    PickerDate.SelectedDate = item.CreateDate;

                    // !!! НОВОЕ: Загрузка текста "Основание" !!!
                    // Если в БД поле называется по-другому (например DocNumber), замени Description на него

                    TxtSeries.Text = item.Series;
                    TxtStart.Text = item.StartNumber.ToString();
                    TxtEnd.Text = item.EndNumber.ToString();

                    if (item.EndNumber >= item.StartNumber)
                        TxtQty.Text = (item.EndNumber - item.StartNumber + 1).ToString();

                    ComboStatus.SelectedValue = item.StatusId;
                    ComboEmp.SelectedValue = item.OwnerId;
                }
            }
            _isLoaded = true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    var item = db.Main_Inventory.Find(_idToEdit);
                    if (item != null)
                    {
                        // 1. Дата
                        if (PickerDate.SelectedDate.HasValue)
                            item.CreateDate = PickerDate.SelectedDate.Value;

                        // 2. !!! НОВОЕ: Сохранение текста "Основание" !!!

                        // 3. Статус
                        if (ComboStatus.SelectedValue != null)
                            item.StatusId = (int)ComboStatus.SelectedValue;

                        // 4. Сотрудник
                        if (item.StatusId == 1) // Если "На складе" (Приход)
                        {
                            item.OwnerId = null;
                        }
                        else
                        {
                            if (ComboEmp.SelectedValue != null)
                                item.OwnerId = (int)ComboEmp.SelectedValue;
                            else
                            {
                                MessageBox.Show("Для расхода/выдачи нужно выбрать сотрудника!", "Внимание");
                                return;
                            }
                        }

                        // 5. Номера
                        item.Series = TxtSeries.Text;
                        if (int.TryParse(TxtStart.Text, out int s) && int.TryParse(TxtEnd.Text, out int en))
                        {
                            item.StartNumber = s;
                            item.EndNumber = en;
                        }
                        else
                        {
                            MessageBox.Show("В номерах должны быть только цифры!");
                            return;
                        }

                        db.SaveChanges();
                        MessageBox.Show("Данные успешно обновлены!");
                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        // Авторасчет (без изменений)
        private void TxtQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            if (int.TryParse(TxtQty.Text, out int qty) && int.TryParse(TxtStart.Text, out int start) && qty > 0)
                TxtEnd.Text = (start + qty - 1).ToString();
        }

        private void TxtStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            if (int.TryParse(TxtQty.Text, out int qty) && int.TryParse(TxtStart.Text, out int start) && qty > 0)
                TxtEnd.Text = (start + qty - 1).ToString();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
    }
}