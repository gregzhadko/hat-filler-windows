using GalaSoft.MvvmLight.Threading;

namespace HatDesktop.Views
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}