using System;
using GalaSoft.MvvmLight.Messaging;
using HatNewUI.UtilsObject;
using HatNewUI.ViewModel;

namespace HatNewUI
{
    public static class MessengerHelper
    {
        public const string DisplayViewToken = "DisplayView";
        public const string DisplayDialogToken = "DisplayDialog";
        public const string CloseViewToken = "CloseView";
        public const string CloseDialogViewToken = "CloseDialog";
        public const string CloseWindowToken = "CloseWindow";

        internal static void RegisterAppMessages<TView>(this IMVVMApp<TView> app)
        {
            Messenger.Default.Register<NotificationMessage<ViewLoadData<TView>>>(app, app.DisplayView);
            Messenger.Default.Register<NotificationMessage<DialogLoadData<TView>>>(app, app.DisplayDialog);
            Messenger.Default.Register<NotificationMessage<WindowShutdownData>>(app, app.CloseWindow);
            Messenger.Default.Register<NotificationMessage<ViewShutdownData>>(app, app.CloseView);
            Messenger.Default.Register<NotificationMessage<DialogShutdownData>>(app, app.CloseDialogView);
        }

        public static void SendDisplayViewMessage<T>(object sender, T view,
            params object[] parameters)
        {
            var data = new ViewLoadData<T>(view, parameters);

            Messenger.Default.Send(new NotificationMessage<ViewLoadData<T>>(
                sender, data, DisplayViewToken));
        }

        public static void SendDisplayDialogMessage<T>(object sender, T view,
            Action<DialogResult> callback, params object[] parameters)
        {
            var data = new DialogLoadData<T>(view, callback, parameters);

            Messenger.Default.Send(new NotificationMessage<DialogLoadData<T>>
                (sender, data, DisplayDialogToken));
        }

        public static void SendCloseMessage(BaseViewModel sender, DialogResult res = null)
        {
            var isDialog = sender.ShownAsDialog.HasValue && sender.ShownAsDialog.Value;

            if (isDialog) SendCloseDialogMessage(sender, res);
            else SendCloseViewMessage(sender);
        }

        static void SendCloseViewMessage(BaseViewModel sender)
        {
            var shutdownData = new ViewShutdownData { ViewModel = sender };

            var message = new NotificationMessage<ViewShutdownData>
                (sender, shutdownData, CloseViewToken);

            Messenger.Default.Send(message);
        }
        static void SendCloseDialogMessage(BaseViewModel sender, DialogResult res)
        {
            var shutdownData = new DialogShutdownData
            {
                Result = res,
                ViewModel = sender,
            };

            var message = new NotificationMessage<DialogShutdownData>
                (sender, shutdownData, CloseDialogViewToken);

            Messenger.Default.Send(message);
        }


        public static void SendCloseWindowMessage(BaseViewModel sender, DialogResult res = null)
        {
            var windowShutdownData = new WindowShutdownData
            {
                Result = res,
                ViewModel = sender,
            };

            var message = new NotificationMessage<WindowShutdownData>
                (sender, windowShutdownData, CloseWindowToken);

            Messenger.Default.Send(message);
        }
    }
}
