using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;



namespace TEST
{
    public partial class MainWindow : Window
    {
        #region Константы и статические поля

        private const string DatabaseConnectionString =
            "Server=localhost;Database=KolobokDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private static readonly Dictionary<string, string> TableDisplayNames = new Dictionary<string, string>
        {
            { "categories", "Категории меню" },
            { "dining_rooms", "Залы" },
            { "employees", "Сотрудники" },
            { "inventory_items", "Складские позиции" },
            { "menu_items", "Блюда меню" },
            { "order_items", "Заказанные блюда" },
            { "orders", "Заказы" },
            { "payment_methods", "Виды оплаты" },
            { "payments", "Платежи" },
            { "recipes", "Рецепты" },
            { "roles", "Роли сотрудников" },
            { "suppliers", "Поставщики" },
            { "tables_seating", "Столы" },
            { "shifts", "Смены" },
            { "stock_movements", "Движения склада" }
        };

        #endregion

        #region Приватные поля

        private Dictionary<string, DataTable> cachedTableData = new Dictionary<string, DataTable>();
        private TelegramLogger? _logger;
        private string? _currentTableName = null;

        #endregion

        #region Конструкторы и инициализация

        public MainWindow() : this(null) { }

        public MainWindow(TelegramLogger? logger)
        {
            InitializeComponent();
            _logger = logger;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            InitializeNavigationButtons();
        }

        /// 
        /// Инициализация обработчиков кнопок навигации
        /// 
        private void InitializeNavigationButtons()
        {
            // Кнопка "Все таблицы"
            BtnAllTables.Click += async (s, e) =>
            {
                _currentTableName = null;
                UpdatePageTitle("Работа с таблицами базы данных", "Просмотр всех таблиц системы одновременно");
                await ShowAllTablesAsync();
            };

            // Секция: Меню и заказы
            BtnCategories.Click += async (s, e) =>
            {
                _currentTableName = "categories";
                UpdatePageTitle("Категории меню", "Управление категориями блюд в меню");
                await ShowSingleTableAsync("categories");
            };

            BtnMenuItems.Click += async (s, e) =>
            {
                _currentTableName = "menu_items";
                UpdatePageTitle("Блюда меню", "Управление блюдами и позициями меню");
                await ShowSingleTableAsync("menu_items");
            };

            BtnRecipes.Click += async (s, e) =>
            {
                _currentTableName = "recipes";
                UpdatePageTitle("Рецепты блюд", "Управление рецептами и ингредиентами блюд");
                await ShowSingleTableAsync("recipes");
            };

            BtnOrders.Click += async (s, e) =>
            {
                _currentTableName = "orders";
                UpdatePageTitle("Заказы", "Управление заказами клиентов");
                await ShowSingleTableAsync("orders");
            };

            BtnOrderItems.Click += async (s, e) =>
            {
                _currentTableName = "order_items";
                UpdatePageTitle("Заказанные блюда", "Детализация заказов по блюдам");
                await ShowSingleTableAsync("order_items");
            };

            // Секция: Платежи
            BtnPaymentMethods.Click += async (s, e) =>
            {
                _currentTableName = "payment_methods";
                UpdatePageTitle("Виды оплаты", "Управление способами оплаты");
                await ShowSingleTableAsync("payment_methods");
            };

            BtnPayments.Click += async (s, e) =>
            {
                _currentTableName = "payments";
                UpdatePageTitle("Платежи", "Управление платежами и транзакциями");
                await ShowSingleTableAsync("payments");
            };

            // Секция: Залы и столы
            BtnDiningRooms.Click += async (s, e) =>
            {
                _currentTableName = "dining_rooms";
                UpdatePageTitle("Залы столовой", "Управление залами обслуживания");
                await ShowSingleTableAsync("dining_rooms");
            };

            BtnTablesSeating.Click += async (s, e) =>
            {
                _currentTableName = "tables_seating";
                UpdatePageTitle("Столы", "Управление столами и посадочными местами");
                await ShowSingleTableAsync("tables_seating");
            };

            // Секция: Складской учёт
            BtnInventoryItems.Click += async (s, e) =>
            {
                _currentTableName = "inventory_items";
                UpdatePageTitle("Складские позиции", "Управление товарами на складе");
                await ShowSingleTableAsync("inventory_items");
            };

            BtnStockMovements.Click += async (s, e) =>
            {
                _currentTableName = "stock_movements";
                UpdatePageTitle("Движения склада", "Учёт поступлений и списаний товаров");
                await ShowSingleTableAsync("stock_movements");
            };

            BtnSuppliers.Click += async (s, e) =>
            {
                _currentTableName = "suppliers";
                UpdatePageTitle("Поставщики", "Управление поставщиками товаров");
                await ShowSingleTableAsync("suppliers");
            };

            // Секция: Персонал
            BtnEmployees.Click += async (s, e) =>
            {
                _currentTableName = "employees";
                UpdatePageTitle("Сотрудники", "Управление персоналом столовой");
                await ShowSingleTableAsync("employees");
            };

            BtnRoles.Click += async (s, e) =>
            {
                _currentTableName = "roles";
                UpdatePageTitle("Роли сотрудников", "Управление ролями и должностями");
                await ShowSingleTableAsync("roles");
            };

            BtnShifts.Click += async (s, e) =>
            {
                _currentTableName = "shifts";
                UpdatePageTitle("Смены", "Управление рабочими сменами");
                await ShowSingleTableAsync("shifts");
            };

            // Установка текущей даты и времени
            CurrentDateText.Text = DateTime.Now.ToString("dd MMMM yyyy, HH:mm");
        }

        #endregion

        #region События окна

        /// 
        /// Обработчик загрузки главного окна
        /// 
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllTableDataAsync();
            await ShowAllTablesAsync();
        }

        /// 
        /// Обработчик закрытия окна с подтверждением и логированием
        /// 
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            var result = CustomMessageBox.Show(
                "Вы действительно хотите выйти из системы учёта?",
                "Подтверждение выхода",
                CustomMessageBox.MessageBoxType.Question,
                CustomMessageBox.MessageBoxButtons.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                if (_logger != null)
                {
                    try
                    {
                        _logger.LogExitSync("Администратор");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка логирования выхода: {ex.Message}");
                    }
                }

                Closing -= MainWindow_Closing;
                Environment.Exit(0);
            }
        }

        #endregion

        #region Методы загрузки и отображения данных

        /// 
        /// Загрузка всех таблиц из базы данных
        /// 
        private async Task LoadAllTableDataAsync()
        {
            UpdateStatus("Загрузка данных из базы...");

            try
            {
                var tables = await GetTableNamesAsync();

                if (!tables.Any())
                {
                    UpdateStatus("Таблицы не найдены.");
                    CustomMessageBox.ShowWarning("В базе данных не найдено ни одной таблицы.", "Внимание");
                    return;
                }

                foreach (var tableName in tables)
                {
                    var tableData = await LoadTableDataAsync(tableName);
                    cachedTableData[tableName] = tableData;
                }

                UpdateStatus($"Данные загружены: {tables.Count} таблиц.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки: {ex.Message}");
                CustomMessageBox.ShowError($"Не удалось загрузить данные из базы:\n{ex.Message}", "Ошибка загрузки");
            }
        }

        /// <summary>
        /// Отображение всех таблиц в одной вкладке + отдельная вкладка с графиками
        /// </summary>
        private Task ShowAllTablesAsync()
        {
            UpdateStatus("Отображение всех таблиц...");
            ClearTabControl();

            try
            {
                // ✅ ВКЛАДКА 1: Все таблицы в одной вкладке
                CreateAllTablesTab();

                // ✅ ВКЛАДКА 2: Графики в отдельной вкладке
                CreateChartsTab();

                if (MainTabControl.Items.Count > 0)
                {
                    MainTabControl.SelectedIndex = 0;
                }

                UpdateStatus($"Отображено {cachedTableData.Count} таблиц.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка: {ex.Message}");
                CustomMessageBox.ShowError($"Ошибка при отображении таблиц:\n{ex.Message}", "Ошибка");
            }

            return Task.CompletedTask;
        }





        /// 
        /// Отображение одной выбранной таблицы
        /// 
        private async Task ShowSingleTableAsync(string tableName)
        {
            UpdateStatus($"Загрузка таблицы: {GetTableDisplayName(tableName)}...");
            ClearTabControl();

            try
            {
                if (!cachedTableData.ContainsKey(tableName))
                {
                    var tableData = await LoadTableDataAsync(tableName);
                    cachedTableData[tableName] = tableData;
                }

                CreateTableTab(tableName);

                if (MainTabControl.Items.Count > 0)
                {
                    MainTabControl.SelectedIndex = 0;
                }

                UpdateStatus($"Таблица '{GetTableDisplayName(tableName)}' отображена.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка: {ex.Message}");
                CustomMessageBox.ShowError($"Не удалось загрузить таблицу:\n{ex.Message}", "Ошибка загрузки");
            }
        }

        #endregion

        #region Методы создания UI элементов

        /// 
        /// Очистка TabControl от всех вкладок
        /// 
        private void ClearTabControl()
        {
            MainTabControl.Items.Clear();
        }

        /// 
        /// Создание вкладки с таблицей данных
        /// 
        private void CreateTableTab(string tableName)
        {
            var tab = CreateTabItem(tableName);
            var dataGrid = CreateDataGrid();
            var tableData = cachedTableData[tableName];

            dataGrid.ItemsSource = tableData.DefaultView;
            dataGrid.Tag = tableName;

            tab.Content = dataGrid;
            MainTabControl.Items.Add(tab);

            AttachEditEventHandler(dataGrid, tableName);
        }

        /// 
        /// Создание элемента вкладки
        /// 
        private TabItem CreateTabItem(string tableName)
        {
            var displayName = GetTableDisplayName(tableName);
            return new TabItem { Header = displayName };
        }

        /// 
        /// Создание DataGrid для отображения данных таблицы
        /// 
        private DataGrid CreateDataGrid()
        {
            return new DataGrid
            {
                AutoGenerateColumns = true,
                IsReadOnly = false,
                EnableRowVirtualization = true,
                Margin = new Thickness(10)
            };
        }

        /// 
        /// Присоединение обработчика редактирования к DataGrid
        /// 
        private void AttachEditEventHandler(DataGrid dataGrid, string tableName)
        {
            dataGrid.BeginningEdit += (s, e) =>
                UpdateStatus($"Изменение в таблице {GetTableDisplayName(tableName)}");
        }


        /// <summary>
        /// Создание вкладки со всеми таблицами
        /// </summary>
        private void CreateAllTablesTab()
        {
            var allTablesTab = new TabItem
            {
                Header = "📋 Все таблицы",
                FontWeight = FontWeights.SemiBold
            };

            // ✅ ГЛАВНЫЙ GRID С СТРОКАМИ
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Кнопка сверху
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // ScrollViewer

            // ✅ КНОПКА ЭКСПОРТА ВСЕХ ТАБЛИЦ (строка 0)
            var exportAllButton = new Button
            {
                Content = "💾 Экспортировать все таблицы в Excel",
                Background = (System.Windows.Media.Brush)FindResource("AccentColor"),
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(20, 12, 20, 12),
                Margin = new Thickness(20, 20, 20, 10),
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            exportAllButton.Click += (s, e) =>
            {
                ExportHelper.ExportAllTables(cachedTableData, TableDisplayNames);
            };

            Grid.SetRow(exportAllButton, 0);
            mainGrid.Children.Add(exportAllButton);

            // ✅ SCROLLVIEWER С ТАБЛИЦАМИ (строка 1)
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(20, 10, 20, 20)
            };

            Grid.SetRow(scrollViewer, 1);

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Создание таблиц
            foreach (var tableName in cachedTableData.Keys.OrderBy(k => k))
            {
                var tableContainer = CreateTableContainer(tableName);
                stackPanel.Children.Add(tableContainer);
            }

            scrollViewer.Content = stackPanel;
            mainGrid.Children.Add(scrollViewer);

            allTablesTab.Content = mainGrid;

            MainTabControl.Items.Add(allTablesTab);
        }



        /// <summary>
        /// Создание контейнера с таблицей
        /// </summary>
        private Border CreateTableContainer(string tableName)
        {
            var border = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("ContentBackground"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor"),
                BorderThickness = new Thickness(1),        // ✅ 1 параметр - все стороны
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 20),      // ✅ 4 параметра
                Padding = new Thickness(20)               // ✅ 1 параметр
            };

            var stackPanel = new StackPanel();

            // Заголовок таблицы
            var title = new TextBlock
            {
                Text = GetTableDisplayName(tableName),
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary"),
                Margin = new Thickness(0, 0, 0, 15)       // ✅ 4 параметра
            };
            stackPanel.Children.Add(title);

            // DataGrid с данными таблицы
            var dataGrid = CreateDataGrid();
            var tableData = cachedTableData[tableName];

            dataGrid.ItemsSource = tableData.DefaultView;
            dataGrid.Tag = tableName;
            dataGrid.MaxHeight = 400;

            stackPanel.Children.Add(dataGrid);

            AttachEditEventHandler(dataGrid, tableName);

            // Кнопка экспорта этой таблицы
            var exportButton = new Button
            {
                Content = $"💾 Экспортировать '{GetTableDisplayName(tableName)}'",
                Background = (System.Windows.Media.Brush)FindResource("SecondaryColor"),
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 13,
                Padding = new Thickness(15, 8, 15, 8),    // ✅ 4 параметра
                Margin = new Thickness(0, 15, 0, 0),      // ✅ 4 параметра
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(0),       // ✅ 1 параметр
                HorizontalAlignment = HorizontalAlignment.Left
            };

            exportButton.Click += (s, e) =>
            {
                ExportHelper.ExportSingleTable(tableName, GetTableDisplayName(tableName), tableData);
            };

            stackPanel.Children.Add(exportButton);

            border.Child = stackPanel;
            return border;
        }






        #endregion

        #region Обработчики CRUD кнопок

        /// 
        /// Обработчик кнопки "Добавить"
        /// 
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var currentGrid = GetCurrentDataGrid();
            if (currentGrid == null) return;

            var tableName = currentGrid.Tag as string;
            if (string.IsNullOrEmpty(tableName)) return;

            var tableData = cachedTableData[tableName];
            ShowAddRecordDialog(tableName, tableData, currentGrid);
        }

        /// 
        /// Обработчик кнопки "Удалить"
        /// 
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var currentGrid = GetCurrentDataGrid();
            if (currentGrid == null) return;

            var tableName = currentGrid.Tag as string;
            if (string.IsNullOrEmpty(tableName)) return;

            var tableData = cachedTableData[tableName];
            await DeleteSelectedRecordAsync(tableName, tableData, currentGrid);
        }

        /// 
        /// Обработчик кнопки "Сохранить"
        /// 
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var currentGrid = GetCurrentDataGrid();
            if (currentGrid == null) return;

            var tableName = currentGrid.Tag as string;
            if (string.IsNullOrEmpty(tableName)) return;

            var tableData = cachedTableData[tableName];
            await SaveChangesAsync(tableName, tableData);
        }

        /// 
        /// Обработчик кнопки "Обновить"
        /// 
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var currentGrid = GetCurrentDataGrid();
            if (currentGrid == null) return;

            var tableName = currentGrid.Tag as string;
            if (string.IsNullOrEmpty(tableName)) return;

            await RefreshTableDataAsync(tableName, currentGrid);
        }

        #endregion

        #region CRUD операции

        /// 
        /// Отображение диалога добавления записи
        /// 
        private async void ShowAddRecordDialog(string tableName, DataTable tableSchema, DataGrid dataGrid)
        {
            try
            {
                var idColumn = FindIdColumn(tableSchema);
                var nextIdValue = idColumn != null
                    ? await GetNextIdValueAsync(tableName, idColumn.ColumnName)
                    : 1;

                var dialog = new AddRecordDialog(
                    tableName,
                    GetTableDisplayName(tableName),
                    tableSchema,
                    nextIdValue)
                {
                    Owner = this
                };

                if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                {
                    var newRow = CreateNewRowFromInput(tableSchema, dialog.InputFields);
                    tableSchema.Rows.Add(newRow);

                    await InsertRecordAsync(tableName, newRow);
                    RefreshDataGrid(dataGrid, tableSchema);

                    UpdateStatus($"Запись добавлена в таблицу {GetTableDisplayName(tableName)}");

                    CustomMessageBox.ShowSuccess(
                        $"✅ Запись успешно добавлена!\n\nТаблица: {GetTableDisplayName(tableName)}",
                        "Добавление выполнено");

                    // Логирование в Telegram
                    if (_logger != null)
                    {
                        await _logger.LogAddRecordAsync(GetTableDisplayName(tableName));
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(
                    $"Ошибка при добавлении записи:\n{ex.Message}",
                    "Ошибка добавления");

                if (_logger != null)
                {
                    await _logger.LogErrorAsync($"Ошибка добавления в {tableName}: {ex.Message}");
                }
            }
        }

        /// 
        /// Удаление выбранной записи из таблицы
        /// 
        private async Task DeleteSelectedRecordAsync(string tableName, DataTable tableSchema, DataGrid dataGrid)
        {
            if (dataGrid.SelectedItem is not DataRowView selectedRowView)
            {
                CustomMessageBox.ShowWarning(
                    "⚠️ Пожалуйста, выберите запись для удаления из таблицы.",
                    "Выбор записи");
                return;
            }

            var result = CustomMessageBox.Show(
                "Вы уверены, что хотите удалить выбранную запись?\n\n⚠️ Это действие нельзя будет отменить.",
                "Подтверждение удаления",
                CustomMessageBox.MessageBoxType.Question,
                CustomMessageBox.MessageBoxButtons.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var selectedRow = selectedRowView.Row;
                var primaryKeyColumn = GetPrimaryKeyColumn(tableSchema);
                var primaryKeyValue = selectedRow[primaryKeyColumn];

                await DeleteRecordFromDatabaseAsync(tableName, primaryKeyColumn.ColumnName, primaryKeyValue);

                tableSchema.Rows.Remove(selectedRow);
                RefreshDataGrid(dataGrid, tableSchema);

                UpdateStatus($"Запись удалена из таблицы {GetTableDisplayName(tableName)}");

                CustomMessageBox.ShowSuccess(
                    $"🗑️ Запись успешно удалена!\n\nТаблица: {GetTableDisplayName(tableName)}\nID записи: {primaryKeyValue}",
                    "Удаление выполнено");

                // Логирование в Telegram
                if (_logger != null)
                {
                    await _logger.LogDeleteRecordAsync(
                        GetTableDisplayName(tableName),
                        primaryKeyValue.ToString());
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(
                    $"Ошибка при удалении записи:\n{ex.Message}",
                    "Ошибка удаления");

                if (_logger != null)
                {
                    await _logger.LogErrorAsync($"Ошибка удаления из {tableName}: {ex.Message}");
                }
            }
        }

        /// 
        /// Сохранение изменений в базу данных
        /// 
        private async Task SaveChangesAsync(string tableName, DataTable tableSchema)
        {
            var modifiedRows = GetModifiedRows(tableSchema);

            if (!modifiedRows.Any())
            {
                CustomMessageBox.ShowInformation(
                    "ℹ️ В таблице нет несохранённых изменений.",
                    "Нет изменений");
                return;
            }

            try
            {
                foreach (var row in modifiedRows)
                {
                    await UpdateRecordAsync(tableName, tableSchema, row);
                }

                tableSchema.AcceptChanges();
                UpdateStatus($"Изменения сохранены в таблице {GetTableDisplayName(tableName)}");

                CustomMessageBox.ShowSuccess(
                    $"💾 Изменения успешно сохранены!\n\nТаблица: {GetTableDisplayName(tableName)}\nИзменено записей: {modifiedRows.Count}",
                    "Сохранение выполнено");

                // Логирование в Telegram
                if (_logger != null)
                {
                    await _logger.LogUpdateRecordsAsync(
                        GetTableDisplayName(tableName),
                        modifiedRows.Count);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(
                    $"Ошибка при сохранении изменений:\n{ex.Message}",
                    "Ошибка сохранения");

                if (_logger != null)
                {
                    await _logger.LogErrorAsync($"Ошибка сохранения в {tableName}: {ex.Message}");
                }
            }
        }

        /// 
        /// Обновление данных таблицы из базы
        /// 
        private async Task RefreshTableDataAsync(string tableName, DataGrid dataGrid)
        {
            UpdateStatus($"Обновление данных таблицы {GetTableDisplayName(tableName)}...");

            try
            {
                var freshData = await LoadTableDataAsync(tableName);
                cachedTableData[tableName] = freshData;
                dataGrid.ItemsSource = freshData.DefaultView;

                UpdateStatus("Данные обновлены.");
                CustomMessageBox.ShowSuccess(
                    $"🔄 Данные таблицы '{GetTableDisplayName(tableName)}' успешно обновлены.",
                    "Обновление завершено");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка обновления: {ex.Message}");
                CustomMessageBox.ShowError(
                    $"Не удалось обновить данные:\n{ex.Message}",
                    "Ошибка обновления");
            }
        }

        #endregion

        #region Методы работы с базой данных

        /// 
        /// Получение списка всех таблиц из базы данных
        /// 
        private async Task<List<string>> GetTableNamesAsync()
        {
            const string query = @"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE' 
                    AND TABLE_CATALOG = 'KolobokDB'
                    AND TABLE_NAME <> 'sysdiagrams'
                ORDER BY TABLE_NAME;";

            var tableNames = new List<string>();

            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tableNames.Add(reader.GetString(0));
            }

            return tableNames;
        }

        /// 
        /// Загрузка данных из таблицы
        /// 
        private async Task<DataTable> LoadTableDataAsync(string tableName)
        {
            var dataTable = new DataTable();
            var query = $"SELECT * FROM [{tableName}]";

            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            dataTable.Load(reader);

            return dataTable;
        }

        /// 
        /// Получение следующего значения ID для новой записи
        /// 
        private async Task<int> GetNextIdValueAsync(string tableName, string idColumnName)
        {
            var query = $"SELECT ISNULL(MAX([{idColumnName}]), 0) FROM [{tableName}]";

            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            var maxId = await command.ExecuteScalarAsync();

            return Convert.ToInt32(maxId) + 1;
        }

        /// 
        /// Вставка новой записи в базу данных
        /// 
        private async Task InsertRecordAsync(string tableName, DataRow record)
        {
            var nonAutoIncrementColumns = GetNonAutoIncrementColumns(record.Table);
            var query = BuildInsertQuery(tableName, nonAutoIncrementColumns);

            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            AddParametersToCommand(command, record, nonAutoIncrementColumns);

            await command.ExecuteNonQueryAsync();
        }

        /// 
        /// Удаление записи из базы данных
        /// 
        private async Task DeleteRecordFromDatabaseAsync(string tableName, string primaryKeyColumn, object primaryKeyValue)
        {
            var query = $"DELETE FROM [{tableName}] WHERE [{primaryKeyColumn}] = @primaryKey";

            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@primaryKey", primaryKeyValue);

            await command.ExecuteNonQueryAsync();
        }

        /// 
        /// Обновление записи в базе данных
        /// 
        private async Task UpdateRecordAsync(string tableName, DataTable tableSchema, DataRow record)
        {
            var primaryKeyColumn = GetPrimaryKeyColumn(tableSchema);
            var primaryKeyValue = record[primaryKeyColumn];

            var updateableColumns = GetUpdateableColumns(tableSchema, primaryKeyColumn);
            var query = BuildUpdateQuery(tableName, updateableColumns, primaryKeyColumn.ColumnName);

            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            AddUpdateParametersToCommand(command, record, updateableColumns, primaryKeyValue);

            await command.ExecuteNonQueryAsync();
        }

        #endregion

        #region Вспомогательные методы для работы с данными

        /// 
        /// Поиск колонки ID в таблице
        /// 
        private DataColumn? FindIdColumn(DataTable tableSchema)
        {
            return tableSchema.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => c.ColumnName.Equals("id", StringComparison.OrdinalIgnoreCase));
        }

        /// 
        /// Создание новой строки данных из введённых значений
        /// 
        private DataRow CreateNewRowFromInput(DataTable tableSchema, Dictionary<string, TextBox> inputFields)
        {
            var row = tableSchema.NewRow();

            foreach (DataColumn column in tableSchema.Columns)
            {
                if (!column.AutoIncrement)
                {
                    var value = inputFields[column.ColumnName].Text;
                    row[column.ColumnName] = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
                }
            }

            return row;
        }

        /// 
        /// Получение списка не-автоинкрементных колонок
        /// 
        private List<DataColumn> GetNonAutoIncrementColumns(DataTable table)
        {
            return table.Columns.Cast<DataColumn>()
                .Where(c => !c.AutoIncrement)
                .ToList();
        }

        /// 
        /// Получение списка изменённых строк
        /// 
        private List<DataRow> GetModifiedRows(DataTable tableSchema)
        {
            return tableSchema.Rows.Cast<DataRow>()
                .Where(row => row.RowState == DataRowState.Modified)
                .ToList();
        }

        /// 
        /// Получение колонки первичного ключа
        /// 
        private DataColumn GetPrimaryKeyColumn(DataTable tableSchema)
        {
            return tableSchema.PrimaryKey.FirstOrDefault() ?? tableSchema.Columns[0];
        }

        /// 
        /// Получение списка обновляемых колонок (без первичного ключа и автоинкремента)
        /// 
        private List<DataColumn> GetUpdateableColumns(DataTable tableSchema, DataColumn primaryKeyColumn)
        {
            return tableSchema.Columns.Cast<DataColumn>()
                .Where(c => !c.AutoIncrement && c.ColumnName != primaryKeyColumn.ColumnName)
                .ToList();
        }

        /// <summary>
        /// Безопасное получение ресурса Brush с резервным значением
        /// </summary>
        private System.Windows.Media.Brush GetResourceBrush(string resourceKey, System.Windows.Media.Color defaultColor)
        {
            try
            {
                return (System.Windows.Media.Brush)FindResource(resourceKey);
            }
            catch
            {
                return new System.Windows.Media.SolidColorBrush(defaultColor);
            }
        }


        #endregion

        #region Методы построения SQL запросов

        /// 
        /// Построение INSERT запроса
        /// 
        private string BuildInsertQuery(string tableName, List<DataColumn> columns)
        {
            var columnNames = string.Join(",", columns.Select(c => $"[{c.ColumnName}]"));
            var parameterNames = string.Join(",", columns.Select(c => $"@{c.ColumnName}"));

            return $"INSERT INTO [{tableName}] ({columnNames}) VALUES ({parameterNames})";
        }

        ///
        /// Построение UPDATE запроса
        ///
        private string BuildUpdateQuery(string tableName, List<DataColumn> columns, string primaryKeyColumn)
        {
            var setClause = string.Join(", ", columns.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}"));
            return $"UPDATE [{tableName}] SET {setClause} WHERE [{primaryKeyColumn}] = @primaryKey";
        }

        ///
        /// Добавление параметров в SQL команду для INSERT
        ///
        private void AddParametersToCommand(SqlCommand command, DataRow record, List<DataColumn> columns)
        {
            foreach (var column in columns)
            {
                var value = record[column.ColumnName] ?? DBNull.Value;
                command.Parameters.AddWithValue($"@{column.ColumnName}", value);
            }
        }

        /// 
        /// Добавление параметров в SQL команду для UPDATE
        /// 
        private void AddUpdateParametersToCommand(
            SqlCommand command,
            DataRow record,
            List<DataColumn> updateableColumns,
            object primaryKeyValue)
        {
            foreach (var column in updateableColumns)
            {
                var value = record[column.ColumnName] ?? DBNull.Value;
                command.Parameters.AddWithValue($"@{column.ColumnName}", value);
            }

            command.Parameters.AddWithValue("@primaryKey", primaryKeyValue);
        }

        #endregion

        #region Методы создания графиков

        /// <summary>
        /// Создание вкладки с графиками для всех таблиц
        /// </summary>
        private void CreateChartsTab()
        {
            var chartsTab = new TabItem
            {
                Header = "📊 Графики",
                FontWeight = FontWeights.SemiBold
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Создание графика для каждой таблицы
            foreach (var tableName in cachedTableData.Keys.OrderBy(k => k))
            {
                var chartContainer = CreateChartContainer(tableName);
                stackPanel.Children.Add(chartContainer);
            }

            scrollViewer.Content = stackPanel;
            chartsTab.Content = scrollViewer;

            MainTabControl.Items.Add(chartsTab);
        }

        /// <summary>
        /// Создание контейнера с графиком для таблицы
        /// </summary>
        private Border CreateChartContainer(string tableName)
        {
            var border = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("ContentBackground"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel();

            // Заголовок графика
            var title = new TextBlock
            {
                Text = $"{GetTableDisplayName(tableName)} - {ChartHelper.GetChartTitle(tableName)}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(title);

            // График
            try
            {
                var tableData = cachedTableData[tableName];
                var series = ChartHelper.CreateChartForTable(tableName, tableData);

                var cartesianChart = new CartesianChart
                {
                    Series = series,
                    Height = 300,
                    XAxes = new LiveChartsCore.Kernel.Sketches.ICartesianAxis[]
                    {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    MinStep = 1,
                    // ✅ ИСПОЛЬЗУЕМ МЕТКИ ИЗ ChartHelper
                    Labels = ChartHelper.CurrentXLabels.Count > 0
                        ? ChartHelper.CurrentXLabels.ToArray()
                        : null
                }
                    },
                    YAxes = new LiveChartsCore.Kernel.Sketches.ICartesianAxis[]
                    {
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    MinStep = 1,
                    Labeler = value => value.ToString("N0")
                }
                    }
                };

                stackPanel.Children.Add(cartesianChart);
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"Ошибка создания графика: {ex.Message}",
                    Foreground = System.Windows.Media.Brushes.Red,
                    FontSize = 12
                };
                stackPanel.Children.Add(errorText);
            }

            border.Child = stackPanel;
            return border;
        }



        #endregion


        #region Вспомогательные методы UI

        /// 
        /// Получение текущей активной DataGrid
        /// 
        private DataGrid? GetCurrentDataGrid()
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                return selectedTab.Content as DataGrid;
            }
            return null;
        }

        /// 
        /// Обновление источника данных DataGrid
        /// 
        private void RefreshDataGrid(DataGrid dataGrid, DataTable dataSource)
        {
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = dataSource.DefaultView;
        }

        /// 
        /// Обновление статусной строки
        /// 
        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        /// 
        /// Обновление заголовка страницы
        /// 
        private void UpdatePageTitle(string title, string subtitle)
        {
            PageTitleText.Text = title;
            PageSubtitleText.Text = subtitle;
        }

        /// 
        /// Получение отображаемого имени таблицы
        /// 
        private string GetTableDisplayName(string tableName)
        {
            return TableDisplayNames.TryGetValue(tableName, out var displayName)
                ? displayName
                : tableName;
        }

        #endregion
    }
}
