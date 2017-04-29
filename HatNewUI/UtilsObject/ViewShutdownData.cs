using MVVMBase;

namespace HatNewUI.UtilsObject
{
    public class ViewShutdownData
    {
        public BaseViewModel ViewModel { get; set; }
    }

    public class DialogShutdownData : ViewShutdownData
    {
        public DialogResult Result { get; set; }
    }
}
