using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TEST
{
    /// <summary>
    /// Сервис для обработки обратных вызовов от Telegram бота
    /// </summary>
    public class TelegramBotService
    {
        #region Приватные поля

        private readonly TelegramBotClient _botClient;
        private readonly CancellationTokenSource _cts;
        private readonly TelegramLogger _logger;

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализация сервиса Telegram бота
        /// </summary>
        public TelegramBotService(string botToken, TelegramLogger logger)
        {
            _botClient = new TelegramBotClient(botToken);
            _cts = new CancellationTokenSource();
            _logger = logger;
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Запуск прослушивания обновлений от бота
        /// </summary>
        public void StartListening()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.CallbackQuery }
            };

            // ✅ ИСПРАВЛЕННЫЙ ВЫЗОВ (без именованного параметра pollingErrorHandler)
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                _cts.Token);
        }


        /// <summary>
        /// Остановка прослушивания
        /// </summary>
        public void StopListening()
        {
            _cts.Cancel();
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработка обновлений от бота
        /// </summary>
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callbackQuery = update.CallbackQuery;
                var callbackData = callbackQuery.Data;

                if (callbackData != null && callbackData.StartsWith("auth_"))
                {
                    // Обрабатываем callback авторизации
                    TelegramLogger.ProcessAuthCallback(callbackData);

                    // Отправляем ответ пользователю
                    var responseText = callbackData.Contains("_yes_")
                        ? "✅ Доступ предоставлен!"
                        : "❌ Доступ отклонён. Войдите через пароль.";

                    await botClient.AnswerCallbackQuery(
                        callbackQuery.Id,
                        responseText,
                        showAlert: true,
                        cancellationToken: cancellationToken);
                }
            }
        }

        /// <summary>
        /// Обработка ошибок
        /// </summary>
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка Telegram бота: {exception.Message}");
            return Task.CompletedTask;
        }

        #endregion
    }
}
