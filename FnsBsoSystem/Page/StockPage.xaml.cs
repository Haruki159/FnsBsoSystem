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
using FnsBsoSystem.Entities;
using FnsBsoSystem.Page;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;



namespace FnsBsoSystem.Page
{
    public partial class StockPage : System.Windows.Controls.Page
    {
        public StockPage()
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
                    var rawData = db.Main_Inventory.ToList();

                    //var inventory = rawData.Select(x => new InventoryViewModel
                    //{
                    //    Id = x.Id,
                    //    // --- ДАТА (Всегда заполнена) ---
                    //    Day = x.CreateDate.HasValue ? x.CreateDate.Value.Day.ToString("00") : "01",
                    //    Month = x.CreateDate.HasValue ? x.CreateDate.Value.Month.ToString("00") : "01",
                    //    Year = x.CreateDate.HasValue ? x.CreateDate.Value.Year.ToString() : "2024",

                    //    // --- ОТ КОГО / КОМУ (Заполняем логично) ---
                    //    PersonName = GetPersonName(x),

                    //    // --- ДОКУМЕНТ ОСНОВАНИЕ (Генерируем красиво) ---
                    //    DocName = GetDocName(x),

                    //    // --- ПРИХОД (Если статус 1 - пишем данные, иначе прочерк) ---
                    //    InQty = x.StatusId == 1 ? (x.EndNumber - x.StartNumber + 1).ToString() : "—",
                    //    InSeries = x.StatusId == 1 ? $"{x.Series} {x.StartNumber}-{x.EndNumber}" : "—",

                    //    // --- РАСХОД (Если статус НЕ 1 - пишем данные, иначе прочерк) ---
                    //    OutQty = x.StatusId != 1 ? (x.EndNumber - x.StartNumber + 1).ToString() : "—",
                    //    OutSeries = x.StatusId != 1 ? $"{x.Series} {x.StartNumber}-{x.EndNumber}" : "—",

                    //    // --- ПОДПИСЬ (Только если выдано) ---
                    //    SignMock = x.StatusId != 1 ? "Подпись" : "—",

                    //    // --- ОСТАТОК (Всегда заполнен) ---
                    //    RemQty = x.StatusId == 1 ? (x.EndNumber - x.StartNumber + 1).ToString() : "0",
                    //    RemSeries = x.StatusId == 1 ? $"{x.Series} {x.StartNumber}-{x.EndNumber}" : "—",

                    //    // Технические поля
                    //    StatusId = x.StatusId,
                    //    RawSeries = x.Series,
                    //    RawType = x.Ref_BlankTypes.TypeName
                    //}).OrderByDescending(i => i.Id).ToList(); // Новые сверху

                    var inventory = rawData.Select(x => new InventoryViewModel
                    {
                        Id = x.Id,
                        // --- ДАТА ---
                        Day = x.CreateDate.HasValue ? x.CreateDate.Value.Day.ToString("00") : "01",
                        Month = x.CreateDate.HasValue ? x.CreateDate.Value.Month.ToString("00") : "01",
                        Year = x.CreateDate.HasValue ? x.CreateDate.Value.Year.ToString() : "2024",

                        // --- ОТ КОГО / КОМУ ---
                        PersonName = GetPersonName(x),

                        // --- ДОКУМЕНТ ---
                        DocName = GetDocName(x),

                        // --- ПРИХОД (Убрали проверку StatusId == 1) ---
                        // Теперь всегда вычисляем количество и серию
                        InQty = (x.EndNumber - x.StartNumber + 1).ToString(),
                        InSeries = $"{x.Series} {x.StartNumber}-{x.EndNumber}",

                        // --- РАСХОД (Убрали проверку StatusId != 1) ---
                        // Теперь здесь тоже всегда выводятся данные
                        OutQty = (x.EndNumber - x.StartNumber + 1).ToString(),
                        OutSeries = $"{x.Series} {x.StartNumber}-{x.EndNumber}",

                        // --- ПОДПИСЬ ---
                        // Тут можно оставить логику, или тоже убрать условие, если нужна подпись везде
                        SignMock = x.StatusId != 1 ? "Подпись" : "—",

                        // --- ОСТАТОК ---
                        // Если нужно, чтобы в остатке тоже всегда цифры были (вместо "0" и прочерка)
                        RemQty = (x.EndNumber - x.StartNumber + 1).ToString(),
                        RemSeries = $"{x.Series} {x.StartNumber}-{x.EndNumber}",

                        // Технические поля
                        StatusId = x.StatusId,
                        RawSeries = x.Series,
                        RawType = x.Ref_BlankTypes.TypeName
                    }).OrderByDescending(i => i.Id).ToList();

                    // Поиск
                    if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
                    {
                        string s = TxtSearch.Text.ToLower();
                        inventory = inventory.Where(x =>
                            x.RawSeries.ToLower().Contains(s) ||
                            x.RawType.ToLower().Contains(s) ||
                            x.PersonName.ToLower().Contains(s)
                        ).ToList();
                    }

                    GridStock.ItemsSource = inventory;
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
        }

        // Логика для имени "От кого / Кому"
        private string GetPersonName(Main_Inventory x)
        {
            if (x.StatusId == 1) // На складе
                return "ООО \"Пермская печатная фабрика\""; // Поставщик
            else if (x.StatusId == 4) // Списано
                return "Комиссия по списанию (Акт)";
            else // Выдано сотруднику
                return x.Main_Employees != null ?
                    $"{x.Main_Employees.LastName} {x.Main_Employees.FirstName.Substring(0, 1)}." : "Сотрудник удален";
        }

        // Логика для названия документа
        public string GetDocName(Main_Inventory x)
        {
            if (x.StatusId == 1) return $"Товарная накладная № {x.Id * 12}"; // Фейковый номер накладной
            if (x.StatusId == 4) return $"Акт уничтожения № {x.Id}/С";
            return $"Требование-накладная № {x.Id}";
        }

        // Класс для отображения
        public class InventoryViewModel
        {
            public int Id { get; set; }
            public string Day { get; set; }
            public string Month { get; set; }
            public string Year { get; set; }
            public string PersonName { get; set; }
            public string DocName { get; set; }
            public string InQty { get; set; }
            public string InSeries { get; set; }
            public string OutQty { get; set; }
            public string OutSeries { get; set; }
            public string SignMock { get; set; }
            public string RemQty { get; set; }
            public string RemSeries { get; set; }
            public int StatusId { get; set; }
            public string RawSeries { get; set; }
            public string RawType { get; set; }
        }

        // КНОПКИ
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadData();
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadData();
        private void ActionAdd(object sender, RoutedEventArgs e) { if (new FnsBsoSystem.ReceiptWindow().ShowDialog() == true) LoadData(); }
        private void ActionIssue(object sender, RoutedEventArgs e) { if (new FnsBsoSystem.IssueWindow().ShowDialog() == true) LoadData(); }
        private void ActionSpoil(object sender, RoutedEventArgs e) { if (new FnsBsoSystem.SpoilWindow().ShowDialog() == true) LoadData(); }

        private void ActionDelete(object sender, RoutedEventArgs e)
        {
            if (GridStock.SelectedItem == null) { MessageBox.Show("Выберите запись!"); return; }
            dynamic selected = GridStock.SelectedItem;
            int id = selected.Id;
            if (MessageBox.Show("Удалить запись из книги?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var db = new IFNS6_BsoSystemEntities())
                {
                    var item = db.Main_Inventory.Find(id);
                    if (item != null) { db.Main_Inventory.Remove(item); db.SaveChanges(); LoadData(); }
                }
            }
        }

        private void ActionEdit(object sender, RoutedEventArgs e)
        {
            if (GridStock.SelectedItem == null) { MessageBox.Show("Выберите запись!"); return; }
            dynamic selected = GridStock.SelectedItem;
            int id = selected.Id;
            FnsBsoSystem.EditInventoryWindow win = new FnsBsoSystem.EditInventoryWindow(id);
            if (win.ShowDialog() == true) LoadData();
        }

        // ЭКСПОРТ В EXCEL (ОБНОВЛЕННЫЙ)
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. ЗАПУСК EXCEL
                Excel.Application excel = new Excel.Application();
                excel.Visible = true; // Показываем окно сразу, чтобы видеть процесс
                Excel.Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
                Excel.Worksheet sheet = (Excel.Worksheet)workbook.Sheets[1];

                // Настройка шрифта для всего листа
                sheet.StandardWidth = 10;
                sheet.Range["A1:Z1000"].Font.Name = "Times New Roman";
                sheet.Range["A1:Z1000"].Font.Size = 10;

                // ==========================================
                // 2. РИСУЕМ "ШАПКУ" ДОКУМЕНТА (КОДЫ И НАЗВАНИЯ)
                // ==========================================

                // Главный заголовок
                sheet.Range["A1:L1"].Merge();
                sheet.Cells[1, 1] = "КНИГА УЧЕТА БЛАНКОВ СТРОГОЙ ОТЧЕТНОСТИ";
                sheet.Cells[1, 1].Font.Bold = true;
                sheet.Cells[1, 1].Font.Size = 14;
                sheet.Cells[1, 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заполняем коды
                sheet.Range["H3:L3"].Merge();
                sheet.Cells[3, 8] = "5918. Межрайонная инспекция Федеральной налоговой";
                sheet.Cells[3, 8].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Range["H4:L4"].Merge();
                sheet.Cells[4, 8] = "службы №6 по Пермскому краю";
                sheet.Cells[4, 8].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Данные об учреждении (Слева)
                sheet.Range["A3:F3"].Merge();
                sheet.Cells[3, 1] = "Учреждение: Межрайонная ИФНС №6 по Пермскому краю";
                sheet.Range["A3:F3"].Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;

                sheet.Range["A4:F4"].Merge();
                sheet.Cells[4, 1] = "Структурное подразделение: Административно-хозяйственный отдел";
                sheet.Range["A4:F4"].Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;

                // ==========================================
                // 3. СЛОЖНАЯ ШАПКА ТАБЛИЦЫ (ДВУХУРОВНЕВАЯ)
                // ==========================================

                int r = 8; // Начинаем таблицу с 8-й строки

                // Верхний уровень заголовков (Объединение ячеек)
                sheet.Range[$"A{r}:C{r}"].Merge(); sheet.Cells[r, 1] = "Дата";
                sheet.Range[$"D{r}:D{r + 1}"].Merge(); sheet.Cells[r, 4] = "От кого получено / Кому отпущено";
                sheet.Range[$"E{r}:E{r + 1}"].Merge(); sheet.Cells[r, 5] = "Основание (Документ)";

                sheet.Range[$"F{r}:G{r}"].Merge(); sheet.Cells[r, 6] = "ПРИХОД";
                sheet.Range[$"H{r}:J{r}"].Merge(); sheet.Cells[r, 8] = "РАСХОД";
                sheet.Range[$"K{r}:L{r}"].Merge(); sheet.Cells[r, 11] = "ОСТАТОК";

                // Нижний уровень заголовков
                r++; // Переходим на 9 строку
                sheet.Cells[r, 1] = "День";
                sheet.Cells[r, 2] = "Мес";
                sheet.Cells[r, 3] = "Год";

                sheet.Cells[r, 6] = "Кол-во";
                sheet.Cells[r, 7] = "Серия и №";

                sheet.Cells[r, 8] = "Кол-во";
                sheet.Cells[r, 9] = "Серия и №";
                sheet.Cells[r, 10] = "Подпись";

                sheet.Cells[r, 11] = "Кол-во";
                sheet.Cells[r, 12] = "Серия и №";

                // Оформление всей шапки таблицы
                Excel.Range headerTable = sheet.Range["A8:L9"];
                headerTable.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                headerTable.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                headerTable.Font.Bold = true;
                headerTable.Borders.LineStyle = Excel.XlLineStyle.xlContinuous; // Сетка
                headerTable.WrapText = true; // Перенос текста

                // ==========================================
                // 4. ЗАПОЛНЕНИЕ ДАННЫМИ ИЗ ПРОГРАММЫ
                // ==========================================
                var items = GridStock.ItemsSource as System.Collections.IEnumerable;
                int currentRow = 10;

                if (items != null)
                {
                    foreach (InventoryViewModel item in items)
                    {
                        sheet.Cells[currentRow, 1] = item.Day;
                        sheet.Cells[currentRow, 2] = item.Month;
                        sheet.Cells[currentRow, 3] = item.Year;

                        sheet.Cells[currentRow, 4] = item.PersonName;
                        sheet.Cells[currentRow, 5] = item.DocName;

                        sheet.Cells[currentRow, 6] = item.InQty;
                        sheet.Cells[currentRow, 7] = item.InSeries;

                        sheet.Cells[currentRow, 8] = item.OutQty;
                        sheet.Cells[currentRow, 9] = item.OutSeries;
                        sheet.Cells[currentRow, 10] = ""; // Место для подписи ручкой

                        sheet.Cells[currentRow, 11] = item.RemQty;
                        sheet.Cells[currentRow, 12] = item.RemSeries;

                        currentRow++;
                    }
                }

                // ==========================================
                // 5. ФИНАЛЬНОЕ ОФОРМЛЕНИЕ
                // ==========================================

                // Рисуем сетку для всех данных
                Excel.Range dataRange = sheet.Range[$"A10:L{currentRow - 1}"];
                dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                // Центрируем даты и цифры
                sheet.Range[$"A10:C{currentRow - 1}"].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Range[$"F10:L{currentRow - 1}"].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Настраиваем ширину колонок, чтобы всё влезло
                sheet.Columns[1].ColumnWidth = 4; // День
                sheet.Columns[2].ColumnWidth = 4; // Мес
                sheet.Columns[3].ColumnWidth = 6; // Год
                sheet.Columns[4].ColumnWidth = 25; // ФИО
                sheet.Columns[5].ColumnWidth = 20; // Документ
                sheet.Columns[7].ColumnWidth = 15; // Серия
                sheet.Columns[9].ColumnWidth = 15; // Серия
                sheet.Columns[12].ColumnWidth = 15; // Серия

                // Альбомная ориентация (так как таблица широкая)
                sheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании Excel: " + ex.Message);
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Блокируем интерфейс, пока идет загрузка
                    Mouse.OverrideCursor = Cursors.Wait;

                    Excel.Application excel = new Excel.Application();
                    Excel.Workbook workbook = excel.Workbooks.Open(openFileDialog.FileName);
                    Excel.Worksheet sheet = (Excel.Worksheet)workbook.Sheets[1];

                    // Начинаем читать с 10-й строки (там начинаются данные в нашей форме)
                    int row = 10;
                    int addedCount = 0;

                    using (var db = new IFNS6_BsoSystemEntities())
                    {
                        // Читаем пока есть дата в первой колонке
                        while (sheet.Cells[row, 1].Value != null)
                        {
                            try
                            {
                                // Считываем данные строки
                                // Колонки Excel: 
                                // 1,2,3=Дата, 4=Кому, 7=Приход(Серия-Номер), 9=Расход(Серия-Номер)

                                string day = Convert.ToString(sheet.Cells[row, 1].Value);
                                string month = Convert.ToString(sheet.Cells[row, 2].Value);
                                string year = Convert.ToString(sheet.Cells[row, 3].Value);

                                // Парсим дату
                                DateTime dateOps;
                                if (!DateTime.TryParse($"{day}.{month}.{year}", out dateOps))
                                    dateOps = DateTime.Now;

                                // --- ПРОВЕРЯЕМ КОЛОНКУ ПРИХОД (Колонка 7) ---
                                string inSeriesRaw = Convert.ToString(sheet.Cells[row, 7].Value);
                                if (!string.IsNullOrWhiteSpace(inSeriesRaw) && inSeriesRaw.Contains("-"))
                                {
                                    // Формат в Excel: "AA 100-200"
                                    // Нам нужно разобрать это обратно
                                    ParseAndAddRecord(db, inSeriesRaw, dateOps, 1, null); // 1 = На складе
                                    addedCount++;
                                }

                                // --- ПРОВЕРЯЕМ КОЛОНКУ РАСХОД (Колонка 9) ---
                                string outSeriesRaw = Convert.ToString(sheet.Cells[row, 9].Value);
                                string personName = Convert.ToString(sheet.Cells[row, 4].Value); // Кому выдано

                                if (!string.IsNullOrWhiteSpace(outSeriesRaw) && outSeriesRaw.Contains("-"))
                                {
                                    // Пытаемся найти сотрудника по Фамилии (простой поиск)
                                    int? empId = null;
                                    if (!string.IsNullOrWhiteSpace(personName))
                                    {
                                        // Берем первое слово (Фамилию)
                                        string lastName = personName.Split(' ')[0];
                                        var emp = db.Main_Employees.FirstOrDefault(x => x.LastName == lastName);
                                        if (emp != null) empId = emp.Id;
                                    }

                                    ParseAndAddRecord(db, outSeriesRaw, dateOps, 2, empId); // 2 = Выдано
                                    addedCount++;
                                }
                            }
                            catch { /* Если строка битая, пропускаем её */ }

                            row++; // Следующая строка
                        }

                        db.SaveChanges(); // Сохраняем всё разом
                    }

                    // Закрываем Excel
                    workbook.Close(false);
                    excel.Quit();

                    // Освобождаем ресурсы (чтобы процесс Excel не висел)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);

                    // Обновляем таблицу в приложении
                    LoadData();
                    Mouse.OverrideCursor = null;
                    MessageBox.Show($"Успешно загружено записей: {addedCount}");
                }
                catch (Exception ex)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Ошибка импорта: " + ex.Message);
                }
            }
        }

        // Вспомогательный метод для разбора строки "AA 100-200"
        private void ParseAndAddRecord(IFNS6_BsoSystemEntities db, string rawString, DateTime date, int statusId, int? ownerId)
        {
            // Ожидаем формат: "СЕРИЯ НАЧАЛО-КОНЕЦ" (например: "КД 100-200")
            // Или "СЕРИЯ НАЧАЛО - КОНЕЦ"

            // 1. Находим тире
            int dashIndex = rawString.IndexOf('-');
            if (dashIndex == -1) return;

            // 2. Разделяем на левую часть ("КД 100") и правую ("200")
            string leftPart = rawString.Substring(0, dashIndex).Trim();
            string endNumStr = rawString.Substring(dashIndex + 1).Trim();

            // 3. Из левой части достаем Серию и Начальный номер
            // Ищем последний пробел
            int lastSpaceIndex = leftPart.LastIndexOf(' ');

            string series = "??";
            string startNumStr = "0";

            if (lastSpaceIndex != -1)
            {
                series = leftPart.Substring(0, lastSpaceIndex).Trim();
                startNumStr = leftPart.Substring(lastSpaceIndex + 1).Trim();
            }
            else
            {
                // Если пробела нет, считаем всё серией (ошибка формата, но чтоб не упало)
                series = leftPart;
            }

            // 4. Пробуем превратить в числа и добавить
            if (int.TryParse(startNumStr, out int start) && int.TryParse(endNumStr, out int end))
            {
                // Создаем запись
                var newItem = new Main_Inventory
                {
                    TypeId = 1, // По умолчанию ставим 1-й тип (Квитанция), так как в Excel тип трудно распознать
                    Series = series,
                    StartNumber = start,
                    EndNumber = end,
                    StatusId = statusId,
                    OwnerId = ownerId,
                    CreateDate = date
                };
                db.Main_Inventory.Add(newItem);
            }
        }
    }
}


//            LoadData();
//        }

//        private void LoadData()
//        {
//            try
//            {
//                using (var db = new IFNS6_BsoSystemEntities()) // <-- ТВОЕ ИМЯ
//                {
//                    var inventory = db.Main_Inventory.ToList().Select(x => new
//                    {
//                        x.Id,
//                        TypeName = x.Ref_BlankTypes.TypeName,
//                        x.Series,
//                        FullDesc = $"{x.Series} {x.StartNumber} — {x.EndNumber}",
//                        Count = x.EndNumber - x.StartNumber + 1,
//                        OwnerName = x.Main_Employees != null ? x.Main_Employees.LastName : "--- Склад ---",
//                        StatusName = x.Ref_BlankStatuses.StatusName
//                    }).ToList();

//                    if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
//                    {
//                        string s = TxtSearch.Text.ToLower();
//                        inventory = inventory.Where(x => x.Series.ToLower().Contains(s) || x.TypeName.ToLower().Contains(s)).ToList();
//                    }
//                    GridStock.ItemsSource = inventory;
//                }
//            }
//            catch { }
//        }

//        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadData();
//        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadData();

//        // --- ОПЕРАЦИИ ---
//        private void ActionAdd(object sender, RoutedEventArgs e) { if (new FnsBsoSystem.ReceiptWindow().ShowDialog() == true) LoadData(); }
//        private void ActionIssue(object sender, RoutedEventArgs e) { if (new FnsBsoSystem.IssueWindow().ShowDialog() == true) LoadData(); }
//        private void ActionSpoil(object sender, RoutedEventArgs e) { if (new FnsBsoSystem.SpoilWindow().ShowDialog() == true) LoadData(); }


//        private void ActionDelete(object sender, RoutedEventArgs e)
//        {
//            if (GridStock.SelectedItem == null) { MessageBox.Show("Выберите запись!"); return; }

//            dynamic selected = GridStock.SelectedItem;
//            int id = selected.Id;

//            if (MessageBox.Show("Удалить эту запись безвозвратно?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
//            {
//                using (var db = new IFNS6_BsoSystemEntities())
//                {
//                    var item = db.Main_Inventory.Find(id);
//                    if (item != null)
//                    {
//                        db.Main_Inventory.Remove(item);
//                        db.SaveChanges();
//                        LoadData();
//                    }
//                }
//            }
//        }

//        private void ActionEdit(object sender, RoutedEventArgs e)
//        {
//            if (GridStock.SelectedItem == null) { MessageBox.Show("Выберите запись!"); return; }

//            dynamic selected = GridStock.SelectedItem;
//            int id = selected.Id;

//            // Открываем окно редактирования
//            FnsBsoSystem.EditInventoryWindow win = new FnsBsoSystem.EditInventoryWindow(id);
//            if (win.ShowDialog() == true)
//            {
//                LoadData();
//            }
//        }

//        // --- EXCEL ---
//        private void BtnExport_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                Excel.Application excel = new Excel.Application();
//                excel.Visible = true;
//                Excel.Workbook wb = excel.Workbooks.Add(System.Reflection.Missing.Value);
//                Excel.Worksheet sheet = (Excel.Worksheet)wb.Sheets[1];

//                sheet.Cells[1, 1] = "Тип";
//                sheet.Cells[1, 2] = "Серия";
//                sheet.Cells[1, 3] = "Диапазон";
//                sheet.Cells[1, 4] = "Владелец";

//                var items = GridStock.ItemsSource as System.Collections.IEnumerable;
//                int row = 2;
//                foreach (dynamic item in items)
//                {
//                    sheet.Cells[row, 1] = item.TypeName;
//                    sheet.Cells[row, 2] = item.Series;
//                    sheet.Cells[row, 3] = item.FullDesc;
//                    sheet.Cells[row, 4] = item.OwnerName;
//                    row++;
//                }
//                sheet.Columns.AutoFit();
//            }
//            catch { }
//        }

//        private void GridStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {

//        }
//    }
//}
