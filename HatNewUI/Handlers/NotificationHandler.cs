using System;
using System.Windows;

namespace HatNewUI.Handlers
{
    public class NotificationHandler
    {
        /// <summary>
        /// Shows a dialog box and waits for the result
        /// </summary>
        /// <param name="messageText"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="defaultResult"></param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageText,
          string caption = "",
          MessageBoxButton button = MessageBoxButton.OK,
          MessageBoxImage icon = MessageBoxImage.None,
          MessageBoxResult defaultResult = MessageBoxResult.None)
        {

            var ownerWindow = new Window { WindowStartupLocation = WindowStartupLocation.CenterScreen, Topmost = true };
            var result = MessageBox.Show(ownerWindow, messageText, caption, button, icon, defaultResult);

            return result;
        }

        /// <summary>
        /// Shows a message from a background thread using the current dispatcher
        /// </summary>
        /// <param name="messageText"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="defaultResult"></param>
        /// <param name="callback"></param>
        public static void ShowBackground(string messageText,
            string caption = "",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            Action<MessageBoxResult> callback = null)
        {
            Application.Current.Dispatcher.Invoke((Action) (() =>
            {
                var ownerWindow = new Window {WindowStartupLocation = WindowStartupLocation.CenterScreen, Topmost = true};
                //callback
                var res = MessageBox.Show(ownerWindow, messageText, caption, button, icon, defaultResult);
                callback?.Invoke(res);
            }), null);
        }

    }
}
