using GalaSoft.MvvmLight.Messaging;

namespace HatNewUI.UtilsObject
{
    public interface IMVVMApp<TView> : IMVVMApp
    {
        void DisplayDialog(NotificationMessage<DialogLoadData<TView>> loadData);
        void DisplayView(NotificationMessage<ViewLoadData<TView>> loadData);
    }

    public interface IMVVMApp
    {
        void CloseView(NotificationMessage<ViewShutdownData> closeData);
        void CloseDialogView(NotificationMessage<DialogShutdownData> closeData);
        void CloseWindow(NotificationMessage<WindowShutdownData> closeData);
    }


}
