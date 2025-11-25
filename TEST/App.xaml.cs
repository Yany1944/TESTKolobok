using System;
using System.Windows;

namespace TEST
{
    public partial class App : Application
    {
        #region Константы

        private const string TELEGRAM_BOT_TOKEN = "TOKEN_BOT";
        private const string TELEGRAM_CHAT_ID = "CHAT_ID";
        private const string PASSWORD_URL = "https://pastebin.com/raw/YOUR_PASSWORD_RAW";
        private const int MAX_LOGIN_ATTEMPTS = 3;
        private const string DEFAULT_USERNAME = "Администратор";

        #endregion

        #region Приватные поля

        private TelegramLogger _logger;
        private TelegramBotService _botService;  // ✅ ДОБАВЬТЕ

 
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeApplication();

            // ✅ ЗАПУСК TELEGRAM БОТА
            _botService = new TelegramBotService(TELEGRAM_BOT_TOKEN, _logger);
            _botService.StartListening();

            if (PerformAuthentication())
            {
                LaunchMainWindow();
            }
            else
            {
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // ✅ ОСТАНОВКА TELEGRAM БОТА
            _botService?.StopListening();

            base.OnExit(e);
        }

        /// <summary>
        /// Получение экземпляра TelegramLogger
        /// </summary>
        public TelegramLogger? GetTelegramLogger()
        {
            return _logger;
        }

        #endregion

        #region Инициализация приложения

        /// <summary>
        /// Инициализация настроек приложения
        /// </summary>
        private void InitializeApplication()
        {
            // Отключение вывода ошибок привязки данных
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level =
                System.Diagnostics.SourceLevels.Critical;

            // Настройка режима завершения приложения
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Инициализация логгера Telegram
            _logger = new TelegramLogger(TELEGRAM_BOT_TOKEN, TELEGRAM_CHAT_ID);

            // ✅ ЗАПУСК TELEGRAM БОТА ДЛЯ ОБРАБОТКИ CALLBACK
            _botService = new TelegramBotService(TELEGRAM_BOT_TOKEN, _logger);
            _botService.StartListening();
        }


        #endregion

        #region Процесс авторизации

        /// <summary>
        /// Выполнение процесса авторизации с ограниченным количеством попыток
        /// </summary>
        /// <returns>True если авторизация успешна, False если отменена или превышен лимит попыток</returns>
        private bool PerformAuthentication()
        {
            int attemptCount = 0;

            while (attemptCount < MAX_LOGIN_ATTEMPTS)
            {
                var authResult = ProcessSingleLoginAttempt(attemptCount + 1);

                if (authResult == AuthenticationResult.Success)
                {
                    return true;
                }
                else if (authResult == AuthenticationResult.Cancelled)
                {
                    return false;
                }

                attemptCount++;
            }

            // Превышено максимальное количество попыток
            HandleMaxAttemptsExceeded();
            return false;
        }

        /// <summary>
        /// Обработка одной попытки входа в систему
        /// </summary>
        /// <param name="attemptNumber">Номер текущей попытки</param>
        /// <returns>Результат попытки авторизации</returns>
        private AuthenticationResult ProcessSingleLoginAttempt(int attemptNumber)
        {
            // Показ окна ввода пароля
            var passwordWindow = new PasswordWindow();
            var dialogResult = passwordWindow.ShowDialog();

            // ✅ ПРОВЕРКА: Если окно закрыто с результатом true - авторизация успешна
            if (dialogResult == true)
            {
                // Авторизация прошла (через пароль или Telegram)
                HandleSuccessfulLogin();
                return AuthenticationResult.Success;
            }

            // Проверка отмены пользователем
            if (dialogResult != true)
            {
                HandleLoginCancellation();
                return AuthenticationResult.Cancelled;
            }

            return AuthenticationResult.Failed;
        }


        /// <summary>
        /// Проверка введённого пароля с удалённым сервером
        /// </summary>
        /// <param name="userPassword">Введённый пароль</param>
        /// <param name="attemptNumber">Номер текущей попытки</param>
        /// <returns>Результат проверки пароля</returns>
        private AuthenticationResult ValidatePassword(string userPassword, int attemptNumber)
        {
            try
            {
                // Получение правильного пароля с сервера
                string correctPassword = PasswordWindow.GetOnlinePassword(PASSWORD_URL);

                if (correctPassword == null)
                {
                    HandleServerConnectionError();
                    return AuthenticationResult.Cancelled;
                }

                // Сравнение паролей
                if (userPassword == correctPassword)
                {
                    HandleSuccessfulLogin();
                    return AuthenticationResult.Success;
                }
                else
                {
                    HandleIncorrectPassword(attemptNumber);
                    return AuthenticationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex);
                return AuthenticationResult.Cancelled;
            }
        }

        #endregion

        #region Обработчики результатов авторизации

        /// <summary>
        /// Обработка успешного входа в систему
        /// </summary>
        private void HandleSuccessfulLogin()
        {
            _ = _logger.LogEntryAsync(DEFAULT_USERNAME);

            CustomMessageBox.ShowSuccess(
                "Добро пожаловать в систему учёта столовой „Колобок“!",
                "Авторизация успешна");
        }

        /// <summary>
        /// Обработка ввода неверного пароля
        /// </summary>
        /// <param name="attemptNumber">Номер текущей попытки</param>
        private void HandleIncorrectPassword(int attemptNumber)
        {
            int remainingAttempts = MAX_LOGIN_ATTEMPTS - attemptNumber;

            _ = _logger.LogErrorAsync(
                $"Неудачная попытка входа: неверный пароль (попытка {attemptNumber}/{MAX_LOGIN_ATTEMPTS})");

            if (remainingAttempts > 0)
            {
                CustomMessageBox.ShowWarning(
                    $"Введён неверный пароль.\n\nОсталось попыток: {remainingAttempts}",
                    "Ошибка авторизации");
            }
        }

        /// <summary>
        /// Обработка отмены входа пользователем
        /// </summary>
        private void HandleLoginCancellation()
        {
            _ = _logger.LogErrorAsync("Авторизация отменена пользователем");
        }

        /// <summary>
        /// Обработка превышения максимального количества попыток входа
        /// </summary>
        private void HandleMaxAttemptsExceeded()
        {
            _ = _logger.LogErrorAsync($"Превышено максимальное количество попыток входа ({MAX_LOGIN_ATTEMPTS})");

            CustomMessageBox.ShowError(
                $"Превышено максимальное количество попыток входа ({MAX_LOGIN_ATTEMPTS}).\n\nПриложение будет закрыто.",
                "Доступ заблокирован");
        }

        /// <summary>
        /// Обработка ошибки подключения к серверу авторизации
        /// </summary>
        private void HandleServerConnectionError()
        {
            _ = _logger.LogErrorAsync("Ошибка подключения к серверу авторизации");

            CustomMessageBox.ShowError(
                "Не удалось подключиться к серверу авторизации.\n\nПроверьте подключение к интернету и попробуйте позже.",
                "Ошибка подключения");
        }

        /// <summary>
        /// Обработка критической ошибки в процессе авторизации
        /// </summary>
        /// <param name="ex">Исключение с информацией об ошибке</param>
        private void HandleCriticalError(Exception ex)
        {
            _ = _logger.LogErrorAsync($"Критическая ошибка при запуске: {ex.Message}");

            CustomMessageBox.ShowError(
                $"Произошла критическая ошибка:\n\n{ex.Message}",
                "Критическая ошибка");
        }

        #endregion

        #region Запуск главного окна

        /// <summary>
        /// Запуск главного окна приложения после успешной авторизации
        /// </summary>
        private void LaunchMainWindow()
        {
            var mainWindow = new MainWindow(_logger);
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }

        #endregion

        #region Перечисления

        /// <summary>
        /// Результаты попытки авторизации
        /// </summary>
        private enum AuthenticationResult
        {
            /// <summary>Успешная авторизация</summary>
            Success,

            /// <summary>Неверный пароль</summary>
            Failed,

            /// <summary>Авторизация отменена пользователем или из-за ошибки</summary>
            Cancelled
        }

        #endregion
    }
}
