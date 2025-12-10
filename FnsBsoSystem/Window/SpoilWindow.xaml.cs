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
                    db.Log_Operations.Add(new Log_Operations
                    {
                        UserId = App.CurrentUserId == 0 ? 1 : App.CurrentUserId,
                        ActionType = "Списание",
                        Details = $"Бланк №{TxtNum.Text} списан. Акт составлен.",
                        OperationDate = DateTime.Now
                    });

                    db.SaveChanges();
                }

                MessageBox.Show("Акт списания сформирован и сохранен в журнале.");
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
    }
}
