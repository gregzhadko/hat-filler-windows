using System;
using HatNewUI.Helpers;
using HatNewUI.IoC;
using HatNewUI.ViewModel;
using HatNewUI.Views;

namespace HatNewUI
{
    public partial class App
    {
        static T NewInstanceFactory<T>() where T : new()
        {
            return new T();
        }

        public static void RegisterServicesOnIoC()
        {
            //VIEWMODELS
            UIIoCContainer.RegisterReal<MainViewModel>();
            UIIoCContainer.RegisterReal<FillerViewModel>();


        }

        void RegisterViewsOnIoC()
        {
            //WINDOWS
            UIIoCContainer.Register(CreateView<MainWindow>);


            //VIEWS
            UIIoCContainer.Register<IDataContextHolder>(CreateView<FillerView>, ViewsEnum.Filler.ToString());
            
        }

        static string GetDialogToken(string originalToken)
        {
            return string.Format("{0}_Dialog", originalToken);
        }
        static string GetWindowToken(string originalToken)
        {
            return string.Format("{0}{1}", originalToken, Guid.NewGuid());
        }
    }
}

