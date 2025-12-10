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
            // ЗАМЕНИ ИМЯ НИЖЕ
            using (var db = new IFNS6_BsoSystemEntities())
            {
                var logs = db.Log_Operations.ToList().OrderByDescending(x => x.OperationDate).Select(x => new
                {
                    x.OperationDate,
                    UserName = x.Sys_Users != null ? x.Sys_Users.Login : "System",
                    x.ActionType,
                    x.Details
                }).ToList();

                GridHistory.ItemsSource = logs;
            }
        }
    }
}

