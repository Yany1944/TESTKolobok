using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TEST
{
    /// <summary>
    /// Вспомогательный класс для получения понятных названий полей таблиц
    /// </summary>
    public static class FieldNameHelper
    {
        #region Словарь понятных названий полей

        private static readonly Dictionary<string, Dictionary<string, string>> TableFieldNames =
            new Dictionary<string, Dictionary<string, string>>
            {
                // Категории меню (categories)
                ["categories"] = new Dictionary<string, string>
                {
                    ["id"] = "ID категории",
                    ["name"] = "Название категории",
                    ["is_active"] = "Активна (да/нет)"
                },

                // Залы (dining_rooms)
                ["dining_rooms"] = new Dictionary<string, string>
                {
                    ["id"] = "ID зала",
                    ["name"] = "Название зала"
                },

                // Сотрудники (employees)
                ["employees"] = new Dictionary<string, string>
                {
                    ["id"] = "ID сотрудника",
                    ["full_name"] = "ФИО сотрудника",
                    ["phone"] = "Телефон",
                    ["email"] = "Email",
                    ["role_id"] = "ID должности",
                    ["status"] = "Статус (active/inactive)"
                },

                // Складские позиции (inventory_items)
                ["inventory_items"] = new Dictionary<string, string>
                {
                    ["id"] = "ID товара",
                    ["name"] = "Название товара",
                    ["sku"] = "Артикул (SKU)",
                    ["uom"] = "Единица измерения",
                    ["min_qty"] = "Минимальный остаток",
                    ["is_active"] = "Активен (да/нет)"
                },

                // Блюда меню (menu_items)
                ["menu_items"] = new Dictionary<string, string>
                {
                    ["id"] = "ID блюда",
                    ["category_id"] = "ID категории",
                    ["name"] = "Название блюда",
                    ["description"] = "Описание блюда",
                    ["sku"] = "Артикул (SKU)",
                    ["price"] = "Цена (руб.)",
                    ["is_active"] = "Доступно (да/нет)"
                },

                // Заказанные блюда (order_items)
                ["order_items"] = new Dictionary<string, string>
                {
                    ["id"] = "ID позиции",
                    ["order_id"] = "ID заказа",
                    ["menu_item_id"] = "ID блюда",
                    ["item_name"] = "Название блюда",
                    ["qty"] = "Количество",
                    ["price"] = "Цена за единицу (руб.)",
                    ["discount_amt"] = "Размер скидки (руб.)",
                    ["line_total"] = "Итого по позиции (руб.)",
                    ["status"] = "Статус (new/in_progress/served)",
                    ["note"] = "Примечание"
                },

                // Заказы (orders)
                ["orders"] = new Dictionary<string, string>
                {
                    ["id"] = "ID заказа",
                    ["table_id"] = "ID стола",
                    ["customer_name"] = "Имя клиента / №чека",
                    ["opened_at"] = "Дата открытия заказа",
                    ["closed_at"] = "Дата закрытия заказа",
                    ["status"] = "Статус (open/closed)",
                    ["waiter_id"] = "ID официанта",
                    ["subtotal"] = "Сумма без скидок (руб.)",
                    ["discount_total"] = "Общая скидка (руб.)",
                    ["tax_total"] = "Налоги (руб.)",
                    ["total"] = "Итоговая сумма (руб.)",
                    ["payment_status"] = "Статус оплаты (paid/unpaid/partial)"
                },

                // Виды оплаты (payment_methods)
                ["payment_methods"] = new Dictionary<string, string>
                {
                    ["id"] = "ID способа оплаты",
                    ["code"] = "Код способа",
                    ["name"] = "Название способа оплаты"
                },

                // Платежи (payments)
                ["payments"] = new Dictionary<string, string>
                {
                    ["id"] = "ID платежа",
                    ["order_id"] = "ID заказа",
                    ["method_id"] = "ID способа оплаты",
                    ["amount"] = "Сумма платежа (руб.)",
                    ["paid_at"] = "Дата и время оплаты",
                    ["txn_ref"] = "Номер транзакции"
                },

                // Рецепты (recipes)
                ["recipes"] = new Dictionary<string, string>
                {
                    ["id"] = "ID рецепта",
                    ["menu_item_id"] = "ID блюда",
                    ["inventory_id"] = "ID ингредиента",
                    ["qty_per_unit"] = "Количество на порцию"
                },

                // Роли сотрудников (roles)
                ["roles"] = new Dictionary<string, string>
                {
                    ["id"] = "ID роли",
                    ["code"] = "Код роли",
                    ["name"] = "Название должности"
                },

                // Смены (shifts)
                ["shifts"] = new Dictionary<string, string>
                {
                    ["id"] = "ID смены",
                    ["opened_by"] = "ID открывшего смену",
                    ["opened_at"] = "Дата открытия смены",
                    ["closed_by"] = "ID закрывшего смену",
                    ["closed_at"] = "Дата закрытия смены",
                    ["cash_open"] = "Наличные при открытии (руб.)",
                    ["cash_close"] = "Наличные при закрытии (руб.)"
                },

                // Движения склада (stock_movements)
                ["stock_movements"] = new Dictionary<string, string>
                {
                    ["id"] = "ID движения",
                    ["inventory_id"] = "ID товара",
                    ["movement_type"] = "Тип (purchase/adjustment/usage)",
                    ["qty"] = "Количество",
                    ["unit_cost"] = "Цена за единицу (руб.)",
                    ["movement_at"] = "Дата и время движения",
                    ["order_item_id"] = "ID позиции заказа",
                    ["supplier_id"] = "ID поставщика",
                    ["reason"] = "Причина / Описание"
                },

                // Поставщики (suppliers)
                ["suppliers"] = new Dictionary<string, string>
                {
                    ["id"] = "ID поставщика",
                    ["name"] = "Название компании",
                    ["phone"] = "Телефон",
                    ["email"] = "Email"
                },

                // Столы (tables_seating)
                ["tables_seating"] = new Dictionary<string, string>
                {
                    ["id"] = "ID стола",
                    ["room_id"] = "ID зала",
                    ["table_no"] = "Номер стола",
                    ["capacity"] = "Вместимость (чел.)",
                    ["status"] = "Статус (free/occupied/reserved)"
                }
            };

        #endregion

        #region Словарь подсказок для полей

        private static readonly Dictionary<string, Dictionary<string, string>> FieldHints =
            new Dictionary<string, Dictionary<string, string>>
            {
                // Категории меню
                ["categories"] = new Dictionary<string, string>
                {
                    ["name"] = "Например: Супы, Салаты, Десерты",
                    ["is_active"] = "TRUE - категория видна в меню, FALSE - скрыта"
                },

                // Блюда меню
                ["menu_items"] = new Dictionary<string, string>
                {
                    ["category_id"] = "Выберите ID из таблицы 'Категории меню'",
                    ["sku"] = "Уникальный артикул, например: SUP-001",
                    ["price"] = "Укажите цену в рублях без копеек",
                    ["is_active"] = "TRUE - блюдо доступно для заказа"
                },

                // Заказы
                ["orders"] = new Dictionary<string, string>
                {
                    ["table_id"] = "Выберите ID из таблицы 'Столы'",
                    ["waiter_id"] = "Выберите ID из таблицы 'Сотрудники'",
                    ["status"] = "open - заказ активен, closed - завершён",
                    ["payment_status"] = "paid - оплачен, unpaid - не оплачен, partial - частично"
                },

                // Заказанные блюда
                ["order_items"] = new Dictionary<string, string>
                {
                    ["order_id"] = "Выберите ID из таблицы 'Заказы'",
                    ["menu_item_id"] = "Выберите ID из таблицы 'Блюда меню'",
                    ["qty"] = "Количество порций",
                    ["status"] = "new - новая, in_progress - готовится, served - подана"
                },

                // Платежи
                ["payments"] = new Dictionary<string, string>
                {
                    ["order_id"] = "Выберите ID из таблицы 'Заказы'",
                    ["method_id"] = "Выберите ID из таблицы 'Виды оплаты'",
                    ["txn_ref"] = "Номер транзакции, например: CARD-0001"
                },

                // Рецепты
                ["recipes"] = new Dictionary<string, string>
                {
                    ["menu_item_id"] = "Выберите ID из таблицы 'Блюда меню'",
                    ["inventory_id"] = "Выберите ID из таблицы 'Складские позиции'",
                    ["qty_per_unit"] = "Количество ингредиента на 1 порцию (в кг/л)"
                },

                // Сотрудники
                ["employees"] = new Dictionary<string, string>
                {
                    ["full_name"] = "Фамилия Имя Отчество",
                    ["phone"] = "Формат: +7-XXX-XXX-XX-XX",
                    ["role_id"] = "Выберите ID из таблицы 'Роли сотрудников'",
                    ["status"] = "active - работает, inactive - уволен"
                },

                // Смены
                ["shifts"] = new Dictionary<string, string>
                {
                    ["opened_by"] = "ID сотрудника, открывшего смену",
                    ["closed_by"] = "ID сотрудника, закрывшего смену",
                    ["cash_open"] = "Наличные в кассе на начало смены (руб.)",
                    ["cash_close"] = "Наличные в кассе на конец смены (руб.)"
                },

                // Движения склада
                ["stock_movements"] = new Dictionary<string, string>
                {
                    ["inventory_id"] = "Выберите ID из таблицы 'Складские позиции'",
                    ["movement_type"] = "purchase - закупка, adjustment - корректировка, usage - списание",
                    ["supplier_id"] = "Выберите ID из таблицы 'Поставщики' (для закупок)",
                    ["reason"] = "Причина движения или номер документа"
                },

                // Складские позиции
                ["inventory_items"] = new Dictionary<string, string>
                {
                    ["sku"] = "Уникальный артикул, например: ING-001",
                    ["uom"] = "кг, л, шт и т.д.",
                    ["min_qty"] = "Минимальный остаток для уведомления о нехватке"
                },

                // Столы
                ["tables_seating"] = new Dictionary<string, string>
                {
                    ["room_id"] = "Выберите ID из таблицы 'Залы'",
                    ["table_no"] = "Номер или название стола, например: W1, T1, №5",
                    ["capacity"] = "Максимальное количество гостей",
                    ["status"] = "free - свободен, occupied - занят, reserved - зарезервирован"
                }
            };

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение понятного названия поля
        /// </summary>
        public static string GetFriendlyFieldName(string tableName, string fieldName)
        {
            if (TableFieldNames.TryGetValue(tableName, out var fields))
            {
                if (fields.TryGetValue(fieldName.ToLower(), out var friendlyName))
                {
                    return friendlyName;
                }
            }

            return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        }

        /// <summary>
        /// Получение подсказки для поля
        /// </summary>
        public static string GetFieldHint(string tableName, string fieldName)
        {
            if (FieldHints.TryGetValue(tableName, out var hints))
            {
                if (hints.TryGetValue(fieldName.ToLower(), out var hint))
                {
                    return hint;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Проверка существования маппинга для таблицы
        /// </summary>
        public static bool HasTableMapping(string tableName)
        {
            return TableFieldNames.ContainsKey(tableName);
        }

        #endregion
    }
}
