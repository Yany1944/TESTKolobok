using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TEST
{
    public partial class CustomMessageBox : Window
    {
        #region Перечисления

        /// <summary>
        /// Типы сообщений с различными визуальными стилями
        /// </summary>
        public enum MessageBoxType
        {
            /// <summary>Успешное выполнение операции (зелёный)</summary>
            Success,

            /// <summary>Информационное сообщение (синий)</summary>
            Information,

            /// <summary>Предупреждение (оранжевый)</summary>
            Warning,

            /// <summary>Ошибка (красный)</summary>
            Error,

            /// <summary>Вопрос пользователю (синий)</summary>
            Question
        }

        /// <summary>
        /// Наборы кнопок для диалоговых окон
        /// </summary>
        public enum MessageBoxButtons
        {
            /// <summary>Только кнопка OK</summary>
            OK,

            /// <summary>Кнопки OK и Отмена</summary>
            OKCancel,

            /// <summary>Кнопки Да и Нет</summary>
            YesNo,

            /// <summary>Кнопки Да, Нет и Отмена</summary>
            YesNoCancel
        }

        #endregion

        #region Константы цветов

        private static readonly Color SuccessColor = Color.FromRgb(39, 174, 96);    // #27AE60
        private static readonly Color InformationColor = Color.FromRgb(52, 152, 219); // #3498DB
        private static readonly Color WarningColor = Color.FromRgb(243, 156, 18);     // #F39C12
        private static readonly Color ErrorColor = Color.FromRgb(231, 76, 60);        // #E74C3C
        private static readonly Color SecondaryButtonColor = Color.FromRgb(149, 165, 166); // #95A5A6

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Результат выбора пользователя в диалоговом окне
        /// </summary>
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализация диалогового окна с заданными параметрами
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="title">Заголовок окна</param>
        /// <param name="type">Тип сообщения (определяет иконку и цвет)</param>
        /// <param name="buttons">Набор отображаемых кнопок</param>
        private CustomMessageBox(string message, string title, MessageBoxType type, MessageBoxButtons buttons)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;

            SetMessageBoxType(type);
            SetButtons(buttons);
        }

        #endregion

        #region Настройка визуального стиля

        /// <summary>
        /// Установка визуального стиля в зависимости от типа сообщения
        /// </summary>
        /// <param name="type">Тип сообщения</param>
        private void SetMessageBoxType(MessageBoxType type)
        {
            var (backgroundColor, icon) = GetTypeStyle(type);

            HeaderBorder.Background = new SolidColorBrush(backgroundColor);
            IconText.Text = icon;
            IconText.Foreground = Brushes.White;
        }

        /// <summary>
        /// Получение стиля (цвета и иконки) для типа сообщения
        /// </summary>
        /// <param name="type">Тип сообщения</param>
        /// <returns>Кортеж с цветом фона и текстом иконки</returns>
        private (Color backgroundColor, string icon) GetTypeStyle(MessageBoxType type)
        {
            return type switch
            {
                MessageBoxType.Success => (SuccessColor, "✓"),
                MessageBoxType.Information => (InformationColor, "ℹ"),
                MessageBoxType.Warning => (WarningColor, "⚠"),
                MessageBoxType.Error => (ErrorColor, "✕"),
                MessageBoxType.Question => (InformationColor, "?"),
                _ => (InformationColor, "ℹ")
            };
        }

        #endregion

        #region Настройка кнопок

        /// <summary>
        /// Установка набора кнопок в диалоговом окне
        /// </summary>
        /// <param name="buttons">Тип набора кнопок</param>
        private void SetButtons(MessageBoxButtons buttons)
        {
            ButtonPanel.Children.Clear();

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    AddButton("OK", MessageBoxResult.OK, isPrimary: true);
                    break;

                case MessageBoxButtons.OKCancel:
                    AddButton("Отмена", MessageBoxResult.Cancel, isPrimary: false);
                    AddButton("OK", MessageBoxResult.OK, isPrimary: true);
                    break;

                case MessageBoxButtons.YesNo:
                    AddButton("Нет", MessageBoxResult.No, isPrimary: false);
                    AddButton("Да", MessageBoxResult.Yes, isPrimary: true);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    AddButton("Отмена", MessageBoxResult.Cancel, isPrimary: false);
                    AddButton("Нет", MessageBoxResult.No, isPrimary: false);
                    AddButton("Да", MessageBoxResult.Yes, isPrimary: true);
                    break;
            }
        }

        /// <summary>
        /// Создание и добавление кнопки в панель кнопок
        /// </summary>
        /// <param name="text">Текст на кнопке</param>
        /// <param name="result">Результат при нажатии кнопки</param>
        /// <param name="isPrimary">Является ли кнопка основной (с акцентным цветом)</param>
        private void AddButton(string text, MessageBoxResult result, bool isPrimary)
        {
            var button = new System.Windows.Controls.Button
            {
                Content = text,
                Style = (Style)FindResource("MessageBoxButtonStyle"),
                Tag = result,
                MinWidth = 80
            };

            // Установка цвета для неосновных кнопок
            if (!isPrimary)
            {
                button.Background = new SolidColorBrush(SecondaryButtonColor);
            }

            button.Click += ButtonClickHandler;
            ButtonPanel.Children.Add(button);
        }

        #endregion

        #region Обработчики событий

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
        /// Обработчик нажатия на кнопку в диалоговом окне
        /// </summary>
        private void ButtonClickHandler(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is MessageBoxResult result)
            {
                Result = result;
                DialogResult = true;
            }
        }

        /// <summary>
        /// Обработчик закрытия окна через кнопку X
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            DialogResult = false;
        }

        #endregion

        #region Публичные статические методы

        /// <summary>
        /// Основной метод для отображения диалогового окна с настраиваемыми параметрами
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="title">Заголовок окна</param>
        /// <param name="type">Тип сообщения</param>
        /// <param name="buttons">Набор кнопок</param>
        /// <returns>Результат выбора пользователя</returns>
        public static MessageBoxResult Show(
            string message,
            string title = "Уведомление",
            MessageBoxType type = MessageBoxType.Information,
            MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            var messageBox = new CustomMessageBox(message, title, type, buttons);
            messageBox.ShowDialog();
            return messageBox.Result;
        }

        /// <summary>
        /// Отображение сообщения об успешном выполнении операции
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="title">Заголовок окна (по умолчанию "Успешно")</param>
        /// <returns>Результат выбора пользователя</returns>
        public static MessageBoxResult ShowSuccess(string message, string title = "Успешно")
        {
            return Show(message, title, MessageBoxType.Success, MessageBoxButtons.OK);
        }

        /// <summary>
        /// Отображение информационного сообщения
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="title">Заголовок окна (по умолчанию "Информация")</param>
        /// <returns>Результат выбора пользователя</returns>
        public static MessageBoxResult ShowInformation(string message, string title = "Информация")
        {
            return Show(message, title, MessageBoxType.Information, MessageBoxButtons.OK);
        }

        /// <summary>
        /// Отображение предупреждающего сообщения
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="title">Заголовок окна (по умолчанию "Внимание")</param>
        /// <returns>Результат выбора пользователя</returns>
        public static MessageBoxResult ShowWarning(string message, string title = "Внимание")
        {
            return Show(message, title, MessageBoxType.Warning, MessageBoxButtons.OK);
        }

        /// <summary>
        /// Отображение сообщения об ошибке
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="title">Заголовок окна (по умолчанию "Ошибка")</param>
        /// <returns>Результат выбора пользователя</returns>
        public static MessageBoxResult ShowError(string message, string title = "Ошибка")
        {
            return Show(message, title, MessageBoxType.Error, MessageBoxButtons.OK);
        }

        /// <summary>
        /// Отображение вопроса пользователю с кнопками Да/Нет
        /// </summary>
        /// <param name="message">Текст вопроса</param>
        /// <param name="title">Заголовок окна (по умолчанию "Вопрос")</param>
        /// <returns>Результат выбора пользователя</returns>
        public static MessageBoxResult ShowQuestion(string message, string title = "Вопрос")
        {
            return Show(message, title, MessageBoxType.Question, MessageBoxButtons.YesNo);
        }

        #endregion
    }
}
