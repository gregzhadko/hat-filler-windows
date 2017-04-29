using System;

namespace HatNewUI
{
    public partial class App
    {
        static T NewInstanceFactory<T>() where T : new()
        {
            return new T();
        }

        static public void RegisterServicesOnIoC()
        {
            //VIEWMODELS
            //UIIoCContainer.RegisterReal<LoginViewModel>();
            

        }

        void RegisterViewsOnIoC()
        {
            //WINDOWS
            //UIIoCContainer.Register(CreateView<MainWindow>);


            //VIEWS
            //UIIoCContainer.Register<IDataContextHolder>(CreateView<AdminSettings>, ViewsEnum.AdminSettings.ToString());
            
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

