using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace TEST
{
    public partial class PasswordWindow : Window
    {
        #region Константы

        private const int PASSWORD_REQUEST_TIMEOUT_SECONDS = 10;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Введённый пользователем пароль
        /// </summary>
        public string Password => passwordBox.Password;

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализация окна ввода пароля
        /// </summary>
        public PasswordWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        /// <summary>
        /// Настройка начального состояния окна
        /// </summary>
        private void InitializeWindow()
        {
            // Установка фокуса на поле пароля
            passwordBox.Focus();

            // Скрытие сообщения об ошибке при старте
            HideErrorMessage();
        }

        #endregion

        #region Обработчики событий кнопок


        /// <summary>
        /// Обработчик нажатия кнопки "Войти через Telegram"
        /// </summary>
        private async void TelegramLoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Отключаем кнопки на время ожидания
                var button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                    button.Content = "⏳ Ожидание подтверждения...";
                }

                ShowErrorMessage("📱 Запрос отправлен в Telegram. Проверьте бот!");

                // Получаем TelegramLogger из App
                var app = Application.Current as App;
                var logger = app?.GetTelegramLogger();

                if (logger == null)
                {
                    ShowErrorMessage("❌ Ошибка: Telegram бот не инициализирован");
                    if (button != null)
                    {
                        button.IsEnabled = true;
                        button.Content = "📱 Войти через Telegram";
                    }
                    return;
                }

                // Запрашиваем авторизацию
                bool granted = await logger.RequestTelegramAuthAsync();

                if (granted)
                {
                    HideErrorMessage();
                    DialogResult = true;
                }
                else
                {
                    ShowErrorMessage("❌ Доступ не предоставлен. Войдите через пароль.");
                    if (button != null)
                    {
                        button.IsEnabled = true;
                        button.Content = "📱 Войти через Telegram";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"❌ Ошибка: {ex.Message}");
                var button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                    button.Content = "📱 Войти через Telegram";
                }
            }
        }

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
        /// Обработчик нажатия кнопки "Войти"
        /// </summary>
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePassword())
            {
                return;
            }

            HideErrorMessage();
            DialogResult = true;
        }
        /// <summary>
        /// Обработчик нажатия кнопки "Отмена"
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Обработчик нажатия кнопки закрытия окна (X)
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

        #region Обработчики событий клавиатуры

        /// <summary>
        /// Обработчик нажатия клавиш в поле пароля
        /// </summary>
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Accept_Click(sender, e);
                    break;

                case Key.Escape:
                    Cancel_Click(sender, e);
                    break;
            }
        }

        #endregion

        #region Валидация

        /// <summary>
        /// Проверка введённого пароля на пустоту
        /// </summary>
        /// <returns>True если пароль заполнен, False если пуст</returns>
        private bool ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                ShowErrorMessage("⚠️ Пожалуйста, введите пароль");
                return false;
            }

            return true;
        }

        #endregion

        #region Управление сообщениями об ошибках

        /// <summary>
        /// Отображение сообщения об ошибке через Popup
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            if (PopupMessage != null && MessagePopup != null)
            {
                PopupMessage.Text = message;
                MessagePopup.IsOpen = true;
            }
        }

        /// <summary>
        /// Скрытие сообщения об ошибке
        /// </summary>
        private void HideErrorMessage()
        {
            if (MessagePopup != null)
            {
                MessagePopup.IsOpen = false;
            }
        }


        #endregion



        #region Публичные методы

        /// <summary>
        /// Очистка поля пароля и установка фокуса
        /// </summary>
        public void ClearPassword()
        {
            passwordBox.Clear();
            passwordBox.Focus();
            HideErrorMessage();
        }

        #endregion

        #region Статические методы получения пароля

        /// <summary>
        /// Получение пароля с удалённого сервера по URL
        /// </summary>
        /// <param name="url">URL адрес для получения пароля</param>
        /// <returns>Пароль в виде строки или null при ошибке</returns>
        public static string? GetOnlinePassword(string url)
        {
            try
            {
                return FetchPasswordFromUrl(url);
            }
            catch (Exception ex)
            {
                LogPasswordFetchError(ex);
                return null;
            }
        }

        /// <summary>
        /// Выполнение HTTP запроса для получения пароля
        /// </summary>
        /// <param name="url">URL адрес</param>
        /// <returns>Полученный пароль</returns>
        private static string FetchPasswordFromUrl(string url)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(PASSWORD_REQUEST_TIMEOUT_SECONDS)
            };

            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().Result;
            return content.Trim();
        }

        /// <summary>
        /// Логирование ошибки получения пароля
        /// </summary>
        /// <param name="ex">Исключение с информацией об ошибке</param>
        private static void LogPasswordFetchError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Ошибка получения пароля с сервера: {ex.Message}");
        }

        #endregion
    }
}
