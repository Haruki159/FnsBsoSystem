using FnsBsoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class IssueWindow
    {
        public IssueWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new IFNS6_BsoSystemEntities())
            {
                // Загружаем пачки ТОЛЬКО со статусом 1 (На складе)
                var packs = db.Main_Inventory.Where(x => x.StatusId == 1).ToList()
                    .Select(x => new
                    {
                        Id = x.Id,
                        FullDesc = $"{x.Series} {x.StartNumber}-{x.EndNumber} ({x.Ref_BlankTypes.TypeName})"
                    }).ToList();

                ComboSource.ItemsSource = packs; // В XAML: DisplayMemberPath="FullDesc" SelectedValuePath="Id"

                // Загружаем сотрудников
                var emps = db.Main_Employees.Where(x => x.IsActive == true).ToList()
                    .Select(x => new
                    {
                        Id = x.Id,
                        FullName = $"{x.LastName} {x.FirstName} ({x.Ref_Positions.PosName})"
                    }).ToList();

                ComboEmp.ItemsSource = emps; // В XAML: DisplayMemberPath="FullName" SelectedValuePath="Id"
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- БЛОК ПРОВЕРОК (ВАЛИДАЦИЯ) ---

                // 1. Проверяем, выбрана ли пачка
                if (ComboSource.SelectedValue == null)
                {
                    MessageBox.Show("Пожалуйста, выберите исходную пачку бланков!",
                                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Останавливаем выполнение, чтобы программа не упала
                }

                // 2. Проверяем, выбран ли сотрудник
                if (ComboEmp.SelectedValue == null)
                {
                    MessageBox.Show("Пожалуйста, выберите сотрудника, которому выдаете бланки!",
                                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Проверяем количество (не пустое ли, является ли числом, больше ли нуля)
                // int.TryParse пытается превратить текст в число. Если не вышло — вернет false.
                if (string.IsNullOrWhiteSpace(TxtQty.Text) || !int.TryParse(TxtQty.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Введите корректное количество (целое число больше 0)!",
                                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- ЕСЛИ ДОШЛИ СЮДА, ЗНАЧИТ ВСЕ ДАННЫЕ ЗАПОЛНЕНЫ ВЕРНО ---

                int packId = (int)ComboSource.SelectedValue;
                int empId = (int)ComboEmp.SelectedValue;
                // int qty у нас уже есть из проверки выше

                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // 1. Находим исходную пачку
                    var pack = db.Main_Inventory.Find(packId);
                    if (pack == null) return;

                    int currentCount = pack.EndNumber - pack.StartNumber + 1;

                    // Дополнительная проверка: хватает ли бланков
                    if (currentCount < qty)
                    {
                        MessageBox.Show($"На складе в этой пачке всего {currentCount} шт. Вы не можете выдать {qty} шт.!", "Ошибка количества");
                        return;
                    }

                    // 2. Создаем новую запись для сотрудника (выданная часть)
                    var userPack = new Main_Inventory
                    {
                        TypeId = pack.TypeId,
                        Series = pack.Series,
                        StartNumber = pack.StartNumber,
                        EndNumber = pack.StartNumber + qty - 1,
                        StatusId = 2, // Выдано
                        OwnerId = empId,
                        CreateDate = DateTime.Now
                    };
                    db.Main_Inventory.Add(userPack);

                    // 3. Обновляем складскую пачку
                    pack.StartNumber = pack.StartNumber + qty;

                    if (pack.StartNumber > pack.EndNumber)
                    {
                        db.Main_Inventory.Remove(pack); // Если выдали всё подчистую
                    }

                    // 4. Лог
                    db.Log_Operations.Add(new Log_Operations
                    {
                        UserId = App.CurrentUserId == 0 ? 1 : App.CurrentUserId,
                        ActionType = "Выдача",
                        Details = $"Выдано {qty} шт. сотруднику ID {empId}",
                        OperationDate = DateTime.Now
                    });

                    db.SaveChanges();
                }

                MessageBox.Show("Выдано успешно!");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Критическая ошибка: " + ex.Message);
            }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Перетаскивание окна мышкой
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}