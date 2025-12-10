using FnsBsoSystem.Class;
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
    public partial class SpoilWindow 
    {
        public SpoilWindow()
        {
            InitializeComponent();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- СБОР ОШИБОК ---
                StringBuilder errors = new StringBuilder();

                if (string.IsNullOrWhiteSpace(TxtNum.Text))
                    errors.AppendLine("- Не указан номер бланка");

                // Если нужно проверить, что это число:
                /*
                if (!string.IsNullOrWhiteSpace(TxtNum.Text) && !int.TryParse(TxtNum.Text, out _))
                    errors.AppendLine("- Номер бланка должен быть числом");
                */

                // Вывод ошибок
                if (errors.Length > 0)
                {
                    MessageBox.Show("Исправьте ошибки:\n\n" + errors.ToString(),
                                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // --- СОХРАНЕНИЕ ---
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // 1. Твое логирование
                    db.Log_Operations.Add(new Log_Operations
                    {
                        UserId = App.CurrentUserId == 0 ? 1 : App.CurrentUserId,
                        ActionType = "Списание",
                        Details = $"Бланк №{TxtNum.Text} списан. Акт составлен.",
                        OperationDate = DateTime.Now
                    });

                    // 2. ВАЖНО: Тут должна быть логика изменения статуса в Main_Inventory
                    // Например: 
                    // var item = db.Main_Inventory.FirstOrDefault(...);
                    // item.StatusId = ...; 

                    db.SaveChanges();
                }

                // 3. НОВАЯ ФИЧА: Генерируем акт сразу после успешного SaveChanges
                GenerateAct(TxtNum.Text, ComboReason.Text);

                MessageBox.Show("Акт списания сформирован и сохранен.");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
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
        private void GenerateAct(string bsoNumber, string reason)
        {
            try
            {
                string templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ActTemplate.docx");

                // ВАЖНО: Проверим, видит ли программа вообще файл
                if (!System.IO.File.Exists(templatePath))
                {
                    MessageBox.Show("ОШИБКА: Файл шаблона не найден по пути: " + templatePath);
                    return;
                }

                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // Убедись, что TxtNum.Text это число
                    if (!int.TryParse(bsoNumber, out int num)) return;

                    var bso = db.Main_Inventory.FirstOrDefault(i => i.StartNumber <= num && i.EndNumber >= num);

                    if (bso == null)
                    {
                        MessageBox.Show("Бланк с таким номером не найден в базе!");
                        return;
                    }

                    var actData = new WriteOffActDto
                    {
                        Series = bso.Series,
                        StartNumber = num,
                        EndNumber = num,
                        Reason = reason,
                        BlankTypeName = bso.Ref_BlankTypes?.TypeName ?? "БСО",
                        EmployeeFullName = bso.Main_Employees?.FullName ?? "Сотрудник",
                        DepartmentName = bso.Main_Employees?.Ref_Departments?.DeptName ?? "Отдел"
                    };

                    var generator = new Class.DocumentGenerator();
                    generator.GenerateWriteOffAct(actData);
                }
            }
            catch (Exception ex)
            {
                // Теперь мы точно увидим, что случилось
                MessageBox.Show("Критическая ошибка генерации: " + ex.Message + "\n\n" + ex.StackTrace);
            }
        }
    }
}
