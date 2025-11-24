using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TEST
{
    public class TelegramLogger
    {
        #region Константы

        private const string APP_HEADER = "🏢 Столовая „Колобок“ - Система учёта";
        private const string TIMESTAMP_FORMAT = "dd.MM.yyyy HH:mm:ss";
        private const int TELEGRAM_TIMEOUT_SECONDS = 3;
        private const string DEFAULT_USERNAME = "Администратор";

        #endregion

        #region Приватные поля

        private readonly string _botToken;
        private readonly string _chatId;
        private readonly string _logFilePath;
        private readonly TelegramBotClient _botClient;

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализация логгера для записи событий в Telegram и файл
        /// </summary>
        /// <param name="botToken">Токен Telegram бота</param>
        /// <param name="chatId">ID чата для отправки сообщений</param>
        /// <param name="logFilePath">Путь к файлу логов (по умолчанию "access_log.txt")</param>
        public TelegramLogger(string botToken, string chatId, string logFilePath = "access_log.txt")
        {
            _botToken = botToken;
            _chatId = chatId;
            _logFilePath = logFilePath;
            _botClient = new TelegramBotClient(_botToken);
        }

        #endregion

        #region Методы логирования входа/выхода

        /// <summary>
        /// Асинхронное логирование входа пользователя в систему
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        public async Task LogEntryAsync(string username = DEFAULT_USERNAME)
        {
            var timestamp = GetCurrentTimestamp();
            var logMessage = $"✅ ВХОД | {username} | {timestamp}";
            var fileMessage = $"[{timestamp}] ВХОД - Пользователь: {username}";

            await WriteToFileAsync(fileMessage);
            await SendTelegramMessageAsync(logMessage);
        }

        /// <summary>
        /// Синхронное логирование выхода пользователя из системы
        /// Используется при завершении приложения для гарантированной отправки
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        public void LogExitSync(string username = DEFAULT_USERNAME)
        {
            var timestamp = GetCurrentTimestamp();
            var logMessage = $"🚪 ВЫХОД | {username} | {timestamp}";
            var fileMessage = $"[{timestamp}] ВЫХОД - Пользователь: {username}";

            WriteToFileSync(fileMessage);
            SendTelegramMessageSync(logMessage);
        }

        #endregion

        #region Методы логирования CRUD операций

        /// <summary>
        /// Логирование добавления новой записи в таблицу
        /// </summary>
        /// <param name="tableName">Название таблицы</param>
        /// <param name="username">Имя пользователя, выполнившего операцию</param>
        public async Task LogAddRecordAsync(string tableName, string username = DEFAULT_USERNAME)
        {
            var timestamp = GetCurrentTimestamp();
            var logMessage = FormatCrudMessage("➕ ДОБАВЛЕНИЕ", username, tableName, timestamp);
            var fileMessage = $"[{timestamp}] ДОБАВЛЕНИЕ - Таблица: {tableName}, Пользователь: {username}";

            await WriteToFileAsync(fileMessage);
            await SendTelegramMessageAsync(logMessage);
        }

        /// <summary>
        /// Логирование удаления записи из таблицы
        /// </summary>
        /// <param name="tableName">Название таблицы</param>
        /// <param name="recordId">ID удалённой записи</param>
        /// <param name="username">Имя пользователя, выполнившего операцию</param>
        public async Task LogDeleteRecordAsync(string tableName, string recordId, string username = DEFAULT_USERNAME)
        {
            var timestamp = GetCurrentTimestamp();
            var logMessage = $"🗑️ УДАЛЕНИЕ | {username}\nТаблица: {tableName}\nID записи: {recordId}\nВремя: {timestamp}";
            var fileMessage = $"[{timestamp}] УДАЛЕНИЕ - Таблица: {tableName}, ID: {recordId}, Пользователь: {username}";

            await WriteToFileAsync(fileMessage);
            await SendTelegramMessageAsync(logMessage);
        }

        /// <summary>
        /// Логирование сохранения изменений в таблице
        /// </summary>
        /// <param name="tableName">Название таблицы</param>
        /// <param name="recordCount">Количество изменённых записей</param>
        /// <param name="username">Имя пользователя, выполнившего операцию</param>
        public async Task LogUpdateRecordsAsync(string tableName, int recordCount, string username = DEFAULT_USERNAME)
        {
            var timestamp = GetCurrentTimestamp();
            var logMessage = $"💾 СОХРАНЕНИЕ | {username}\nТаблица: {tableName}\nИзменено записей: {recordCount}\nВремя: {timestamp}";
            var fileMessage = $"[{timestamp}] СОХРАНЕНИЕ - Таблица: {tableName}, Изменено: {recordCount}, Пользователь: {username}";

            await WriteToFileAsync(fileMessage);
            await SendTelegramMessageAsync(logMessage);
        }

        #endregion

        #region Методы логирования ошибок

        /// <summary>
        /// Логирование ошибки в системе
        /// </summary>
        /// <param name="errorMessage">Описание ошибки</param>
        public async Task LogErrorAsync(string errorMessage)
        {
            var timestamp = GetCurrentTimestamp();
            var logMessage = $"❌ ОШИБКА | {timestamp}\n{errorMessage}";
            var fileMessage = $"[{timestamp}] ОШИБКА - {errorMessage}";

            await WriteToFileAsync(fileMessage);
            await SendTelegramMessageAsync(logMessage);
        }

        #endregion

        #region Асинхронные методы записи

        /// <summary>
        /// Асинхронная запись сообщения в файл логов
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        private async Task WriteToFileAsync(string message)
        {
            try
            {
                await File.AppendAllTextAsync(_logFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                LogToDebug($"Ошибка записи в файл: {ex.Message}");
            }
        }

        /// <summary>
        /// Асинхронная отправка сообщения в Telegram
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        private async Task SendTelegramMessageAsync(string message)
        {
            try
            {
                var fullMessage = FormatTelegramMessage(message);
                await _botClient.SendMessage(chatId: _chatId, text: fullMessage);
            }
            catch (Exception ex)
            {
                LogToDebug($"Ошибка отправки в Telegram: {ex.Message}");
            }
        }

        #endregion

        #region Синхронные методы записи

        /// <summary>
        /// Синхронная запись сообщения в файл логов
        /// Используется при завершении приложения
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        private void WriteToFileSync(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                LogToDebug($"Ошибка записи в файл: {ex.Message}");
            }
        }

        /// <summary>
        /// Синхронная отправка сообщения в Telegram с таймаутом
        /// Используется при завершении приложения для гарантированной отправки
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        private void SendTelegramMessageSync(string message)
        {
            try
            {
                var fullMessage = FormatTelegramMessage(message);
                var task = _botClient.SendMessage(chatId: _chatId, text: fullMessage);

                if (!task.Wait(TimeSpan.FromSeconds(TELEGRAM_TIMEOUT_SECONDS)))
                {
                    LogToDebug("Таймаут отправки сообщения в Telegram");
                }
            }
            catch (Exception ex)
            {
                LogToDebug($"Ошибка отправки в Telegram: {ex.Message}");
            }
        }

        #endregion

        #region Вспомогательные методы форматирования

        /// <summary>
        /// Получение текущей метки времени в формате "дд.ММ.гггг ЧЧ:мм:сс"
        /// </summary>
        /// <returns>Строка с текущей датой и временем</returns>
        private static string GetCurrentTimestamp()
        {
            return DateTime.Now.ToString(TIMESTAMP_FORMAT);
        }

        /// <summary>
        /// Форматирование сообщения для Telegram с заголовком приложения
        /// </summary>
        /// <param name="message">Основной текст сообщения</param>
        /// <returns>Форматированное сообщение</returns>
        private static string FormatTelegramMessage(string message)
        {
            return $"{APP_HEADER}\n\n{message}";
        }

        /// <summary>
        /// Форматирование сообщения о CRUD операции
        /// </summary>
        /// <param name="operation">Тип операции (с эмодзи)</param>
        /// <param name="username">Имя пользователя</param>
        /// <param name="tableName">Название таблицы</param>
        /// <param name="timestamp">Метка времени</param>
        /// <returns>Форматированное сообщение</returns>
        private static string FormatCrudMessage(string operation, string username, string tableName, string timestamp)
        {
            return $"{operation} | {username}\nТаблица: {tableName}\nВремя: {timestamp}";
        }

        /// <summary>
        /// Вывод отладочного сообщения в консоль
        /// </summary>
        /// <param name="message">Сообщение для вывода</param>
        private static void LogToDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        #endregion

        #region Методы авторизации через Telegram

        private static string? _pendingAuthSessionId = null;
        private static bool _authGranted = false;

        /// <summary>
        /// Запрос авторизации через Telegram с кнопками
        /// </summary>
        public async Task<bool> RequestTelegramAuthAsync()
        {
            try
            {
                // Генерируем уникальный ID сессии
                _pendingAuthSessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _authGranted = false;

                // Создаём клавиатуру с кнопками
                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                    new[]
                    {
                new[]
                {
                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
                        "✅ Разрешить доступ", $"auth_yes_{_pendingAuthSessionId}"),
                    Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
                        "❌ Отклонить", $"auth_no_{_pendingAuthSessionId}")
                }
                    });

                var message = $"🔐 Запрос на вход в систему\n\n" +
                             $"🏢 Столовая Колобок - Система учёта\n" +
                             $"🕐 {GetCurrentTimestamp()}\n\n" +
                             $"❓ Открыть доступ?";

                await _botClient.SendMessage(
                    chatId: _chatId,
                    text: message,
                    replyMarkup: keyboard);

                // Ожидаем ответ в течение 60 секунд
                var timeout = DateTime.Now.AddSeconds(60);

                while (DateTime.Now < timeout)
                {
                    if (_authGranted)
                    {
                        await SendTelegramMessageAsync($"✅ Доступ предоставлен!\n\nПользователь: Администратор\nВремя: {GetCurrentTimestamp()}");
                        return true;
                    }

                    if (_pendingAuthSessionId == null)
                    {
                        // Доступ был отклонён
                        return false;
                    }

                    await Task.Delay(500); // Проверяем каждые 0.5 секунд
                }

                // Таймаут
                _pendingAuthSessionId = null;
                await SendTelegramMessageAsync($"⏱️ Время ожидания авторизации истекло\n\nВремя: {GetCurrentTimestamp()}");
                return false;
            }
            catch (Exception ex)
            {
                LogToDebug($"Ошибка запроса авторизации: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Обработка ответа на запрос авторизации (вызывается из обработчика callback)
        /// </summary>
        public static void ProcessAuthCallback(string callbackData)
        {
            if (string.IsNullOrEmpty(_pendingAuthSessionId))
                return;

            if (callbackData.StartsWith($"auth_yes_{_pendingAuthSessionId}"))
            {
                _authGranted = true;
                _pendingAuthSessionId = null;
            }
            else if (callbackData.StartsWith($"auth_no_{_pendingAuthSessionId}"))
            {
                _authGranted = false;
                _pendingAuthSessionId = null;
            }
        }

        /// <summary>
        /// Проверка статуса авторизации
        /// </summary>
        public bool IsAuthGranted()
        {
            return _authGranted;
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получение полного пути к файлу логов
        /// </summary>
        /// <returns>Абсолютный путь к файлу логов</returns>
        public string GetLogFilePath()
        {
            return Path.GetFullPath(_logFilePath);
        }

        #endregion
    }
}
