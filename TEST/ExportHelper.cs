using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using ClosedXML.Excel;

namespace TEST
{
    /// <summary>
    /// Вспомогательный класс для экспорта данных таблиц в различные форматы
    /// </summary>
    public static class ExportHelper
    {
        #region Константы

        private const string EXCEL_FILTER = "Excel файлы (*.xlsx)|*.xlsx|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
        private const string CSV_FILTER = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";

        #endregion

        #region Экспорт одной таблицы

        /// <summary>
        /// Экспорт одной таблицы в выбранный формат
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="displayName">Отображаемое имя таблицы</param>
        /// <param name="tableData">Данные таблицы</param>
        public static void ExportSingleTable(string tableName, string displayName, DataTable tableData)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = EXCEL_FILTER,
                    FileName = $"{displayName}_{DateTime.Now:yyyy-MM-dd_HH-mm}",
                    Title = $"Экспорт таблицы: {displayName}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var extension = Path.GetExtension(saveDialog.FileName).ToLower();

                    if (extension == ".xlsx")
                    {
                        ExportToExcel(saveDialog.FileName, new[] { (displayName, tableData) });
                    }
                    else if (extension == ".csv")
                    {
                        ExportToCsv(saveDialog.FileName, tableData);
                    }

                    CustomMessageBox.ShowSuccess(
                        $"✅ Таблица '{displayName}' успешно экспортирована!\n\nФайл: {saveDialog.FileName}",
                        "Экспорт завершён");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(
                    $"Ошибка при экспорте таблицы:\n{ex.Message}",
                    "Ошибка экспорта");
            }
        }

        #endregion

        #region Экспорт всех таблиц

        /// <summary>
        /// Экспорт всех таблиц в один Excel файл
        /// </summary>
        /// <param name="tables">Словарь таблиц (имя → данные)</param>
        /// <param name="tableDisplayNames">Словарь отображаемых имён</param>
        public static void ExportAllTables(
            System.Collections.Generic.Dictionary<string, DataTable> tables,
            System.Collections.Generic.Dictionary<string, string> tableDisplayNames)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    FileName = $"Все_таблицы_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx",
                    Title = "Экспорт всех таблиц"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var tablesWithNames = tables
                        .OrderBy(kvp => kvp.Key)
                        .Select(kvp =>
                        {
                            var displayName = tableDisplayNames.ContainsKey(kvp.Key)
                                ? tableDisplayNames[kvp.Key]
                                : kvp.Key;
                            return (displayName, kvp.Value);
                        })
                        .ToArray();

                    ExportToExcel(saveDialog.FileName, tablesWithNames);

                    CustomMessageBox.ShowSuccess(
                        $"✅ Все таблицы ({tables.Count} шт.) успешно экспортированы!\n\nФайл: {saveDialog.FileName}",
                        "Экспорт завершён");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(
                    $"Ошибка при экспорте таблиц:\n{ex.Message}",
                    "Ошибка экспорта");
            }
        }

        #endregion

        #region Приватные методы экспорта

        /// <summary>
        /// Экспорт данных в Excel файл с использованием ClosedXML
        /// </summary>
        private static void ExportToExcel(string filePath, (string name, DataTable data)[] tables)
        {
            using var workbook = new XLWorkbook();

            foreach (var (name, data) in tables)
            {
                // Создание листа для каждой таблицы
                var worksheet = workbook.Worksheets.Add(SanitizeSheetName(name));

                // Добавление данных из DataTable
                var table = worksheet.Cell(1, 1).InsertTable(data);

                // Форматирование заголовков
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Автоподбор ширины колонок
                worksheet.Columns().AdjustToContents();

                // Добавление фильтров
                table.Theme = XLTableTheme.TableStyleMedium2;
            }

            // Сохранение файла
            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// Экспорт данных в CSV файл
        /// </summary>
        private static void ExportToCsv(string filePath, DataTable data)
        {
            var csv = new StringBuilder();

            // Заголовки
            var headers = data.Columns.Cast<DataColumn>()
                .Select(column => EscapeCsvValue(column.ColumnName));
            csv.AppendLine(string.Join(";", headers));

            // Данные
            foreach (DataRow row in data.Rows)
            {
                var values = row.ItemArray
                    .Select(value => value == DBNull.Value ? "" : EscapeCsvValue(value.ToString()));
                csv.AppendLine(string.Join(";", values));
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Очистка имени листа Excel от недопустимых символов
        /// </summary>
        private static string SanitizeSheetName(string name)
        {
            // Excel не допускает некоторые символы в именах листов
            var invalidChars = new[] { '/', '\\', '?', '*', '[', ']', ':' };
            var sanitized = name;

            foreach (var ch in invalidChars)
            {
                sanitized = sanitized.Replace(ch, '_');
            }

            // Максимальная длина имени листа в Excel - 31 символ
            if (sanitized.Length > 31)
            {
                sanitized = sanitized.Substring(0, 31);
            }

            return sanitized;
        }

        /// <summary>
        /// Экранирование значения для CSV
        /// </summary>
        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Если содержит спецсимволы - оборачиваем в кавычки
            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        #endregion
    }
}
