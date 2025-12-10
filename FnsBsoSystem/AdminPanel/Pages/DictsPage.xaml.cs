using FnsBsoSystem.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FnsBsoSystem.AdminPanel.Pages
{
    public partial class DictsPage
    {
        private string _currentTable = "";
        public class DictItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public DictsPage() => InitializeComponent();

        private void LoadDepts(object sender, RoutedEventArgs e)
        {
            _currentTable = "Depts";
            using (var db = new IFNS6_BsoSystemEntities())
            {
                // Используем новый класс DictItem
                dgDicts.ItemsSource = db.Ref_Departments
                    .Select(x => new DictItem { Id = x.Id, Name = x.DeptName }).ToList();
            }
        }

        private void LoadPos(object sender, RoutedEventArgs e)
        {
            _currentTable = "Pos";
            using (var db = new IFNS6_BsoSystemEntities())
            {
                dgDicts.ItemsSource = db.Ref_Positions
                    .Select(x => new DictItem { Id = x.Id, Name = x.PosName }).ToList();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    // Берем всё, что сейчас в таблице
                    var items = dgDicts.ItemsSource as List<DictItem>;

                    foreach (var item in items)
                    {
                        // Ищем реальный объект в базе по ID
                        if (_currentTable == "Depts")
                        {
                            var entity = db.Ref_Departments.Find(item.Id);
                            if (entity != null) entity.DeptName = item.Name;
                        }
                        else if (_currentTable == "Pos")
                        {
                            var entity = db.Ref_Positions.Find(item.Id);
                            if (entity != null) entity.PosName = item.Name;
                        }
                    }
                    db.SaveChanges();
                    MessageBox.Show("Сохранено!");
                }
            }
            catch (System.Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}