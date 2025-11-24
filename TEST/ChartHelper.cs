using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace TEST
{
    /// <summary>
    /// Вспомогательный класс для создания графиков на основе данных таблиц
    /// </summary>
    public static class ChartHelper
    {
        #region Константы цветов

        private static readonly SKColor[] ChartColors = new[]
        {
            SKColor.Parse("#3498DB"), // Синий
            SKColor.Parse("#27AE60"), // Зелёный
            SKColor.Parse("#E74C3C"), // Красный
            SKColor.Parse("#F39C12"), // Оранжевый
            SKColor.Parse("#9B59B6"), // Фиолетовый
            SKColor.Parse("#1ABC9C"), // Бирюзовый
            SKColor.Parse("#34495E"), // Тёмно-серый
            SKColor.Parse("#E67E22")  // Морковный
        };

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Метки для оси X (ID из таблицы или порядковые номера)
        /// </summary>
        public static List<string> CurrentXLabels { get; private set; } = new List<string>();

        #endregion

        #region Методы создания графиков для таблиц

        /// <summary>
        /// Создание графика на основе данных таблицы
        /// </summary>
        public static ISeries[] CreateChartForTable(string tableName, DataTable tableData)
        {
            try
            {
                CurrentXLabels.Clear(); // Очищаем предыдущие метки

                return tableName switch
                {
                    "categories" => CreateCategoriesChart(tableData),
                    "menu_items" => CreateMenuItemsChart(tableData),
                    "orders" => CreateOrdersChart(tableData),
                    "payments" => CreatePaymentsChart(tableData),
                    "employees" => CreateEmployeesChart(tableData),
                    "inventory_items" => CreateInventoryChart(tableData),
                    "stock_movements" => CreateStockMovementsChart(tableData),
                    "order_items" => CreateOrderItemsChart(tableData),
                    _ => CreateGenericChart(tableData)
                };
            }
            catch (Exception)
            {
                return CreateGenericChart(tableData);
            }
        }

        #endregion

        #region Графики для конкретных таблиц

        private static ISeries[] CreateCategoriesChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            CurrentXLabels = new List<string> { "Категории" };

            return new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Количество категорий",
                    Values = new[] { data.Rows.Count },
                    Fill = new SolidColorPaint(ChartColors[0])
                }
            };
        }

        private static ISeries[] CreateMenuItemsChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            try
            {
                if (!data.Columns.Contains("price")) return CreateGenericChart(data);

                var items = data.AsEnumerable()
                    .Where(row => row["price"] != DBNull.Value)
                    .Take(10)
                    .Select(row => new
                    {
                        Id = data.Columns.Contains("id") && row["id"] != DBNull.Value
                            ? row["id"].ToString()
                            : "",
                        Name = data.Columns.Contains("name") && row["name"] != DBNull.Value
                            ? row["name"].ToString()
                            : "",
                        Price = Convert.ToDouble(row["price"])
                    })
                    .ToArray();

                if (items.Length == 0) return CreateGenericChart(data);

                // Формируем метки: ID или имя или порядковый номер
                CurrentXLabels = items.Select((item, index) =>
                {
                    if (!string.IsNullOrEmpty(item.Id))
                        return $"ID{item.Id}";
                    else if (!string.IsNullOrEmpty(item.Name))
                        return item.Name.Length > 10 ? item.Name.Substring(0, 10) + "..." : item.Name;
                    else
                        return $"#{index + 1}";
                }).ToList();

                return new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Цены блюд",
                        Values = items.Select(x => x.Price).ToArray(),
                        Fill = new SolidColorPaint(ChartColors[1])
                    }
                };
            }
            catch
            {
                return CreateGenericChart(data);
            }
        }

        private static ISeries[] CreateOrdersChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            try
            {
                if (!data.Columns.Contains("total")) return CreateGenericChart(data);

                var orders = data.AsEnumerable()
                    .Where(row => row["total"] != DBNull.Value)
                    .OrderByDescending(row => data.Columns.Contains("id") ? Convert.ToInt32(row["id"]) : 0)
                    .Take(20)
                    .Reverse()
                    .Select(row => new
                    {
                        Id = data.Columns.Contains("id") && row["id"] != DBNull.Value
                            ? row["id"].ToString()
                            : "",
                        Total = Convert.ToDouble(row["total"])
                    })
                    .ToArray();

                if (orders.Length == 0) return CreateGenericChart(data);

                CurrentXLabels = orders.Select((order, index) =>
                    !string.IsNullOrEmpty(order.Id) ? $"#{order.Id}" : $"#{index + 1}"
                ).ToList();

                return new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Name = "Сумма заказов",
                        Values = orders.Select(x => x.Total).ToArray(),
                        Fill = null,
                        Stroke = new SolidColorPaint(ChartColors[2]) { StrokeThickness = 3 },
                        GeometrySize = 8
                    }
                };
            }
            catch
            {
                return CreateGenericChart(data);
            }
        }

        private static ISeries[] CreatePaymentsChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            try
            {
                if (!data.Columns.Contains("amount")) return CreateGenericChart(data);

                var payments = data.AsEnumerable()
                    .Where(row => row["amount"] != DBNull.Value)
                    .Take(15)
                    .Select((row, index) => new
                    {
                        Id = data.Columns.Contains("id") && row["id"] != DBNull.Value
                            ? row["id"].ToString()
                            : (index + 1).ToString(),
                        Amount = Convert.ToDouble(row["amount"])
                    })
                    .ToArray();

                if (payments.Length == 0) return CreateGenericChart(data);

                CurrentXLabels = payments.Select(p => $"#{p.Id}").ToList();

                return new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Суммы платежей",
                        Values = payments.Select(x => x.Amount).ToArray(),
                        Fill = new SolidColorPaint(ChartColors[3])
                    }
                };
            }
            catch
            {
                return CreateGenericChart(data);
            }
        }

        private static ISeries[] CreateEmployeesChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            CurrentXLabels = new List<string> { "Сотрудники" };

            return new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Количество сотрудников",
                    Values = new[] { data.Rows.Count },
                    Fill = new SolidColorPaint(ChartColors[4])
                }
            };
        }

        private static ISeries[] CreateInventoryChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            try
            {
                // Ищем столбец с количеством
                string quantityColumn = null;
                foreach (var colName in new[] { "current_stock", "stock", "qty", "quantity", "min_qty" })
                {
                    if (data.Columns.Contains(colName))
                    {
                        quantityColumn = colName;
                        break;
                    }
                }

                if (quantityColumn == null) return CreateGenericChart(data);

                var items = data.AsEnumerable()
                    .Where(row => row[quantityColumn] != DBNull.Value)
                    .Take(10)
                    .Select((row, index) => new
                    {
                        Label = data.Columns.Contains("name") && row["name"] != DBNull.Value
                            ? row["name"].ToString()
                            : data.Columns.Contains("id") && row["id"] != DBNull.Value
                                ? $"ID{row["id"]}"
                                : $"#{index + 1}",
                        Quantity = Convert.ToInt32(row[quantityColumn])
                    })
                    .ToArray();

                if (items.Length == 0) return CreateGenericChart(data);

                CurrentXLabels = items.Select(x =>
                    x.Label.Length > 15 ? x.Label.Substring(0, 15) + "..." : x.Label
                ).ToList();

                return new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Name = "Остатки на складе",
                        Values = items.Select(x => x.Quantity).ToArray(),
                        Fill = new SolidColorPaint(ChartColors[5])
                    }
                };
            }
            catch
            {
                return CreateGenericChart(data);
            }
        }

        private static ISeries[] CreateStockMovementsChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            try
            {
                if (!data.Columns.Contains("qty")) return CreateGenericChart(data);

                var movements = data.AsEnumerable()
                    .Where(row => row["qty"] != DBNull.Value)
                    .Take(20)
                    .Select((row, index) => new
                    {
                        Label = data.Columns.Contains("id") && row["id"] != DBNull.Value
                            ? $"#{row["id"]}"
                            : $"#{index + 1}",
                        Quantity = Convert.ToInt32(row["qty"])
                    })
                    .ToArray();

                if (movements.Length == 0) return CreateGenericChart(data);

                CurrentXLabels = movements.Select(x => x.Label).ToList();

                return new ISeries[]
                {
                    new LineSeries<int>
                    {
                        Name = "Движения склада",
                        Values = movements.Select(x => x.Quantity).ToArray(),
                        Fill = null,
                        Stroke = new SolidColorPaint(ChartColors[6]) { StrokeThickness = 3 }
                    }
                };
            }
            catch
            {
                return CreateGenericChart(data);
            }
        }

        private static ISeries[] CreateOrderItemsChart(DataTable data)
        {
            if (data.Rows.Count == 0) return Array.Empty<ISeries>();

            try
            {
                // Ищем столбец с количеством
                string quantityColumn = null;
                foreach (var colName in new[] { "qty", "quantity", "count", "amount" })
                {
                    if (data.Columns.Contains(colName))
                    {
                        quantityColumn = colName;
                        break;
                    }
                }

                if (quantityColumn == null) return CreateGenericChart(data);

                var items = data.AsEnumerable()
                    .Where(row => row[quantityColumn] != DBNull.Value)
                    .Take(8)
                    .Select((row, index) => new
                    {
                        Label = data.Columns.Contains("item_name") && row["item_name"] != DBNull.Value
                            ? row["item_name"].ToString()
                            : data.Columns.Contains("id") && row["id"] != DBNull.Value
                                ? $"ID{row["id"]}"
                                : $"#{index + 1}",
                        Quantity = Convert.ToInt32(row[quantityColumn])
                    })
                    .ToArray();

                if (items.Length == 0) return CreateGenericChart(data);

                CurrentXLabels = items.Select(x =>
                    x.Label.Length > 12 ? x.Label.Substring(0, 12) + "..." : x.Label
                ).ToList();

                return new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Name = "Количество",
                        Values = items.Select(x => x.Quantity).ToArray(),
                        Fill = new SolidColorPaint(ChartColors[7])
                    }
                };
            }
            catch
            {
                return CreateGenericChart(data);
            }
        }

        private static ISeries[] CreateGenericChart(DataTable data)
        {
            CurrentXLabels = new List<string> { "Записи" };

            return new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Количество записей",
                    Values = new[] { data.Rows.Count },
                    Fill = new SolidColorPaint(ChartColors[7])
                }
            };
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Получение заголовка для графика
        /// </summary>
        public static string GetChartTitle(string tableName)
        {
            return tableName switch
            {
                "categories" => "Статистика категорий",
                "menu_items" => "Цены блюд меню",
                "orders" => "Динамика заказов",
                "payments" => "Статистика платежей",
                "employees" => "Количество сотрудников",
                "inventory_items" => "Остатки на складе",
                "stock_movements" => "Движения склада",
                "order_items" => "Заказанные блюда",
                _ => "Статистика"
            };
        }

        #endregion
    }
}
