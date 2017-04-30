using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HatNewUI.Handlers;
using HatNewUI.Helpers;
using MVVMBase;

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
                    Task.Run(
                        () =>
                            NotificationHandler.ShowBackground("Are you sure you want to exit?", "Confirmation", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, callback: x =>
                                {
                                    if (x != MessageBoxResult.Yes) return;
                                    _closingAccepted = true;
                                    MessengerHelper.SendCloseWindowMessage(dc);
                                    Environment.Exit(0);
                                }));
                
            };
        }

        public void SetNewOverheadContent(ViewsEnum viewsEnum, IDataContextHolder newContent)
        {
            AttachedProperties.SetCurrentMainWindowSection(this, viewsEnum);
            OverheadContent.Content = newContent;
        }
    }
}
