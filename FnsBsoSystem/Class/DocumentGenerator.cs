using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Words.NET;

namespace FnsBsoSystem.Class
{
    public class DocumentGenerator
    {
        public void GenerateWriteOffAct(WriteOffActDto data)
        {
            try
            {
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ActTemplate.docx");
                string fileName = $"Акт_списания_БСО_{data.Series}_{data.StartNumber}.docx";
                string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                if (!File.Exists(templatePath))
                {
                    MessageBox.Show("Шаблон акта не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (DocX document = DocX.Load(templatePath))
                {
                    // Заменяем все метки. Если поле пустое, ставим прочерк, чтобы не было "null"
                    document.ReplaceText("{ActNumber}", "9242"); // Или динамический номер
                    document.ReplaceText("{Date}", DateTime.Now.ToString("dd.MM.yyyy"));
                    document.ReplaceText("{DepartmentName}", data.DepartmentName ?? "—");
                    document.ReplaceText("{EmployeeFullName}", data.EmployeeFullName ?? "—");
                    document.ReplaceText("{Position}", data.PositionName ?? "Сотрудник");
                    document.ReplaceText("{NumbersRange}", $"{data.StartNumber}"); // Твой номер 222
                    document.ReplaceText("{Series}", data.Series ?? "—");
                    document.ReplaceText("{Reason}", data.Reason ?? "—");
                    document.ReplaceText("{DestroyDate}", DateTime.Now.ToString("dd.MM.yyyy"));

                    document.SaveAs(outputPath);
                }

                if (MessageBox.Show("Акт сформирован на Рабочем столе!\nОткрыть документ?", "Успех", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
