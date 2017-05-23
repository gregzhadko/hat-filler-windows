using System;
using System.Windows;
using HatNewUI.Helpers;
using HatNewUI.ViewModel;

namespace HatNewUI
{
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDataContextHolder
    {
        private bool _closingAccepted;

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

            Closing += (s, e) =>
            {
                var dc = DataContext as BaseViewModel;
                if (dc == null || _closingAccepted) return;
                e.Cancel = true;
                _closingAccepted = true;
                MessengerHelper.SendCloseWindowMessage(dc);
                Environment.Exit(0);

            };
        }

        public void SetNewOverheadContent(ViewsEnum viewsEnum, IDataContextHolder newContent)
        {
            AttachedProperties.SetCurrentMainWindowSection(this, viewsEnum);
            OverheadContent.Content = newContent;
        }
    }
}
