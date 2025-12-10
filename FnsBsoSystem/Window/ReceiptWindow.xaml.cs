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
using System.Data.Entity;
using FnsBsoSystem.Entities;

namespace FnsBsoSystem
{
    public partial class ReceiptWindow 
    {
        public ReceiptWindow()
        {
            InitializeComponent();
            LoadCombo();
        }

        private void LoadCombo()
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    ComboTypes.ItemsSource = db.Ref_BlankTypes.ToList();
                }
            }
            catch { }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- СБОР ОШИБОК ---
                StringBuilder errors = new StringBuilder();

                // 1. Проверка заполнения полей
                if (ComboTypes.SelectedValue == null)
                    errors.AppendLine("- Не выбран тип бланка");

                if (string.IsNullOrWhiteSpace(TxtSeries.Text))
                    errors.AppendLine("- Не указана серия");

                if (string.IsNullOrWhiteSpace(TxtStart.Text))
                    errors.AppendLine("- Не указан начальный номер");

                if (string.IsNullOrWhiteSpace(TxtEnd.Text))
                    errors.AppendLine("- Не указан конечный номер");

                // 2. Проверка формата чисел (если поля не пустые)
                int start = 0, end = 0;
                bool isStartOk = int.TryParse(TxtStart.Text, out start);
                bool isEndOk = int.TryParse(TxtEnd.Text, out end);

                if (!string.IsNullOrWhiteSpace(TxtStart.Text) && !isStartOk)
                    errors.AppendLine("- Начальный номер должен быть целым числом");

                if (string.IsNullOrWhiteSpace(TxtStart.Text) && !isStartOk)
                    errors.AppendLine("- Начальный номер должен быть целым числом");

                if (!string.IsNullOrWhiteSpace(TxtEnd.Text) && !isEndOk)
                    errors.AppendLine("- Конечный номер должен быть целым числом");

                // 2.1. Проверка на положительность чисел (добавлено по вашему запросу)
                if (isStartOk && start <= 0)
                    errors.AppendLine("- Начальный номер должен быть положительным");

                if (isEndOk && end <= 0)
                    errors.AppendLine("- Конечный номер должен быть положительным");

                // 3. Логическая проверка (только если числа корректны)
                if (isStartOk && isEndOk && start > end)
                    errors.AppendLine("- Начальный номер не может быть больше Конечного");


                // --- ВЫВОД ОШИБОК ---
                if (errors.Length > 0)
                {
                    MessageBox.Show("Пожалуйста, исправьте следующие ошибки:\n\n" + errors.ToString(),
                                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Прерываем сохранение
                }


                // --- СОХРАНЕНИЕ (Если ошибок нет) ---
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    var newItem = new Main_Inventory
                    {
                        TypeId = (int)ComboTypes.SelectedValue,
                        Series = TxtSeries.Text,
                        StartNumber = start,
                        EndNumber = end,
                        StatusId = 1, // На складе
                        OwnerId = null,
                        CreateDate = DateTime.Now
                    };

                    db.Main_Inventory.Add(newItem);

                    db.Log_Operations.Add(new Log_Operations
                    {
                        UserId = App.CurrentUserId == 0 ? 1 : App.CurrentUserId,
                        ActionType = "Приход",
                        Details = $"Поступление: {newItem.Series} {newItem.StartNumber}-{newItem.EndNumber}",
                        OperationDate = DateTime.Now
                    });

                    db.SaveChanges();
                }

                MessageBox.Show("Бланки успешно приняты на склад!");
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

        // Перетаскивать окно мышкой
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}

