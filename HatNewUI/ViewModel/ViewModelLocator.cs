using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using HatNewUI.IoC;
using Microsoft.Practices.ServiceLocation;

namespace HatNewUI.ViewModel
{
    public class ViewModelLocator
    {

        public ViewModelLocator()
        {
            if (ViewModelBase.IsInDesignModeStatic)
                App.RegisterServicesOnIoC();
        }

        public MainViewModel Main => UIIoCContainer.GetInstance<MainViewModel>();
        
    }
}