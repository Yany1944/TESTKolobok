using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TEST
{
    public partial class AddRecordDialog : Window
    {
        #region Приватные поля

        private Dictionary<string, TextBox> _inputFields;
        private DataTable _tableSchema;
        private string _tableName; 

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Словарь полей ввода для каждой колонки таблицы
        /// </summary>
        public Dictionary<string, TextBox> InputFields => _inputFields;

        /// <summary>
        /// Флаг подтверждения добавления записи
        /// </summary>
        public bool IsConfirmed { get; private set; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализация диалогового окна добавления записи
        /// </summary>
        /// <param name="tableName">Системное имя таблицы</param>
        /// <param name="displayName">Отображаемое имя таблицы</param>
        /// <param name="tableSchema">Схема таблицы с описанием колонок</param>
        /// <param name="nextId">Следующий доступный ID для новой записи</param>
        public AddRecordDialog(string tableName, string displayName, DataTable tableSchema, int nextId)
        {
            InitializeComponent();

            _tableSchema = tableSchema;
            _tableName = tableName;
            _inputFields = new Dictionary<string, TextBox>();

            // Установка заголовков окна
            TitleText.Text = "➕ Добавить запись";
            SubtitleText.Text = $"Таблица: {displayName}";

            CreateInputFields(nextId);
        }

        #endregion

        #region Создание полей ввода

        /// <summary>
        /// Создание полей ввода для всех колонок таблицы
        /// </summary>
        /// <param name="nextId">Значение ID для новой записи</param>
        private void CreateInputFields(int nextId)
        {
            var idColumn = FindIdColumn();

            foreach (DataColumn column in _tableSchema.Columns)
            {
                CreateFieldLabel(column);
                var textBox = CreateFieldTextBox(column, idColumn, nextId);
                CreateFieldHint(column);

                _inputFields[column.ColumnName] = textBox;
            }
        }

        /// <summary>
        /// Поиск колонки ID в схеме таблицы
        /// </summary>
        /// <returns>Колонка ID или null</returns>
        private DataColumn? FindIdColumn()
        {
            return _tableSchema.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => c.ColumnName.Equals("id", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Создание метки (label) для поля ввода
        /// </summary>
        /// <param name="column">Колонка таблицы</param>
        private void CreateFieldLabel(DataColumn column)
        {
            // ✅ ИСПОЛЬЗУЕМ ПОНЯТНОЕ НАЗВАНИЕ
            var friendlyName = FieldNameHelper.GetFriendlyFieldName(_tableName, column.ColumnName);

            var label = new TextBlock
            {
                Text = friendlyName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetResourceBrush("TextPrimary", System.Windows.Media.Color.FromRgb(44, 62, 80)),
                Margin = new Thickness(0, 5, 0, 5)
            };

            FieldsPanel.Children.Add(label);
        }


        /// <summary>
        /// Создание текстового поля для ввода данных
        /// </summary>
        /// <param name="column">Колонка таблицы</param>
        /// <param name="idColumn">Колонка ID (для определения автозаполнения)</param>
        /// <param name="nextId">Значение следующего ID</param>
        /// <returns>Созданное текстовое поле</returns>
        private TextBox CreateFieldTextBox(DataColumn column, DataColumn? idColumn, int nextId)
        {
            var textBox = new TextBox
            {
                Style = (Style)FindResource("ModernTextBoxStyle")
            };

            // Настройка для ID и автоинкремента
            if (column.AutoIncrement || column == idColumn)
            {
                textBox.IsReadOnly = true;
                textBox.Background = GetResourceBrush("ContentBackground", System.Windows.Media.Color.FromRgb(240, 240, 240));


                if (column == idColumn)
                {
                    textBox.Text = nextId.ToString();
                }
            }

            FieldsPanel.Children.Add(textBox);
            return textBox;
        }

        /// <summary>
        /// Создание подсказки с информацией о типе данных и обязательности поля
        /// </summary>
        private void CreateFieldHint(DataColumn column)
        {
            var requiredText = column.AllowDBNull ? "" : " • Обязательное";
            var typeText = GetFriendlyTypeName(column.DataType);

            // ✅ ПОЛУЧАЕМ КАСТОМНУЮ ПОДСКАЗКУ
            var customHint = FieldNameHelper.GetFieldHint(_tableName, column.ColumnName);

            var hintText = !string.IsNullOrEmpty(customHint)
                ? $"{customHint}{requiredText}"
                : $"Тип: {typeText}{requiredText}";

            var hint = new TextBlock
            {
                Text = hintText,
                FontSize = 11,
                Foreground = GetResourceBrush("TextSecondary", System.Windows.Media.Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(0, -10, 0, 10),
                TextWrapping = TextWrapping.Wrap  // ✅ Многострочные подсказки
            };

            FieldsPanel.Children.Add(hint);
        }




        #endregion

        #region Валидация данных

        /// <summary>
        /// Проверка заполнения всех обязательных полей
        /// </summary>
        /// <returns>True если все обязательные поля заполнены</returns>
        private bool ValidateRequiredFields()
        {
            var emptyRequired = _tableSchema.Columns.Cast<DataColumn>()
                .Where(c => !c.AllowDBNull && !c.AutoIncrement)
                .Where(c => string.IsNullOrWhiteSpace(_inputFields[c.ColumnName].Text))
                .ToList();

            if (emptyRequired.Any())
            {
                ShowValidationError(emptyRequired);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Отображение ошибки валидации с перечислением незаполненных полей
        /// </summary>
        /// <param name="emptyFields">Список незаполненных обязательных полей</param>
        private void ShowValidationError(List<DataColumn> emptyFields)
        {
            var fieldNames = string.Join("\n", emptyFields.Select(c => $"• {c.ColumnName}"));

            CustomMessageBox.ShowWarning(
                $"Пожалуйста, заполните все обязательные поля:\n\n{fieldNames}",
                "Незаполненные поля");
        }

        #endregion

        #region Обработчики событий кнопок


        /// <summary>
        /// Обработчик перемещения окна при клике на заголовок
        /// </summary>
        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить"
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateRequiredFields())
            {
                return;
            }

            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Отмена"
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Обработчик нажатия кнопки закрытия окна (X)
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        #endregion

        #region Вспомогательные методы

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

        /// <summary>
        /// Получение понятного имени типа данных
        /// </summary>
        private string GetFriendlyTypeName(Type dataType)
        {
            return dataType.Name switch
            {
                "String" => "Текст",
                "Int32" => "Целое число",
                "Int64" => "Большое число",
                "Decimal" => "Число с дробной частью",
                "DateTime" => "Дата и время",
                "Boolean" => "True/False",
                "Byte" => "Байт",
                "menu_item_id" => "Укажите id категории",
                _ => dataType.Name
            };
        }

        #endregion

    }
}
