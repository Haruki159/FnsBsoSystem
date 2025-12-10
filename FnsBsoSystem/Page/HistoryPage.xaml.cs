using FnsBsoSystem.Entities;
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
    public partial class HistoryPage : System.Windows.Controls.Page
    {
        public HistoryPage()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            using (var db = new IFNS6_BsoSystemEntities())
            {
                var logs = (from log in db.Log_Operations
                                // 1. Присоединяем таблицу пользователей (через UserId)
                            join user in db.Sys_Users on log.UserId equals user.Id into userGroup
                            from user in userGroup.DefaultIfEmpty()

                                // 2. Присоединяем таблицу сотрудников (через EmployeeId в таблице пользователей)
                            join emp in db.Main_Employees on (user != null ? user.EmployeeId : 0) equals emp.Id into empGroup
                            from emp in empGroup.DefaultIfEmpty()

                            orderby log.OperationDate descending
                            select new
                            {
                                log.OperationDate,
                                // Если сотрудник найден, берем его FullName, если нет — пишем "Система"
                                UserName = emp != null ? emp.FullName : "Системная запись",
                                log.ActionType,
                                log.Details
                            }).ToList();

                GridHistory.ItemsSource = logs;
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt";
            saveFileDialog.FileName = $"Logs_{DateTime.Now:yyyyMMdd_HHmm}.log";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Заголовок файла
                        sw.WriteLine($"=== ОТЧЕТ ПО ОПЕРАЦИЯМ: {DateTime.Now:dd.MM.yyyy HH:mm} ===");
                        sw.WriteLine(new string('-', 80));

                        // Шапка таблицы (для удобства чтения)
                        sw.WriteLine($"{"Дата",-20} | {"Пользователь",-15} | {"Действие",-20} | {"Подробности"}");
                        sw.WriteLine(new string('-', 80));

                        // Получаем данные из DataGrid
                        var data = GridHistory.ItemsSource as System.Collections.IEnumerable;

                        foreach (var item in data)
                        {
                            // Используем Reflection для получения значений, так как тип объекта анонимный
                            var props = item.GetType().GetProperties();

                            string date = props[0].GetValue(item)?.ToString() ?? "";
                            string user = props[1].GetValue(item)?.ToString() ?? "";
                            string action = props[2].GetValue(item)?.ToString() ?? "";
                            string details = props[3].GetValue(item)?.ToString() ?? "";

                            // Записываем строку с выравниванием (форматирование через интерполяцию строк)
                            sw.WriteLine($"{date,-20} | {user,-15} | {action,-20} | {details}");
                        }
                    }

                    MessageBox.Show("Логи успешно экспортированы!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при записи файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

