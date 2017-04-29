using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HatNewUI.Helpers;
using HatNewUI.UtilsObject;

namespace HatNewUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IMVVMApp<ViewsEnum>
    {
        [STAThread]
        public static void Main()
        {
            RunApplication();
        }

        private static void RunApplication()
        {
            var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.InitializeComponent();
            RegisterServicesOnIoC();
            app.RegisterViewsOnIoC();
            app.LoadTheme();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadMainWindow();
        }

        internal static void LoadMainWindow()
        {
            MessengerHelper.SendDisplayViewMessage(null, ViewsEnum.MainWindow);
        }
    }
}
