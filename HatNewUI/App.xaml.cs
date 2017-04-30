using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using HatNewUI.Helpers;
using HatNewUI.IoC;
using HatNewUI.UtilsObject;
using MVVMBase;

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

        #region View Control Helper Methods

        private static T CreateView<T>() where T : IDataContextHolder, new()
        {
            var view = new T();
            var model = view.DataContext as BaseViewModel;
            if (model != null)
            {
                model.VisualTree = view;
            }
            return view;
        }

        private bool _processingDialogDisplay;

        private readonly Queue<NotificationMessage<DialogLoadData<ViewsEnum>>> _dialogDisplayMessageQueue =
            new Queue<NotificationMessage<DialogLoadData<ViewsEnum>>>();

        public void DisplayDialog(NotificationMessage<DialogLoadData<ViewsEnum>> loadData)
        {
            _dialogDisplayMessageQueue.Enqueue(loadData);

            if (_processingDialogDisplay)
            {
                return;
            }

            try
            {
                _processingDialogDisplay = true;
                while (_dialogDisplayMessageQueue.Count > 0)
                {
                    InnerDisplayDialog(_dialogDisplayMessageQueue.Dequeue());
                }
            }
            catch (Exception ex)
            {
                _dialogDisplayMessageQueue.Clear();
                throw;
            }
            finally
            {
                _processingDialogDisplay = false;
                if (_dialogDisplayMessageQueue.Count > 0)
                {
                    throw new Exception("There are still items on the Dialog Display Message Queue.");
                }
            }
        }

        public MainWindow RealMainWindow
        {
            get
            {
                //return _mainWindow ?? (
                //    _mainWindow = UIIoCContainer.GetInstance<MainWindow>(GetWindowToken(ViewsEnum.MainWindow.ToString())));
                return Windows.OfType<MainWindow>().Single();
            }
        }

        void InnerDisplayDialog(NotificationMessage<DialogLoadData<ViewsEnum>> loadData)
        {
            if (loadData == null || loadData.Content == null ||
               loadData.Notification != MessengerHelper.DisplayDialogToken)
                return;

            var dialogToken = GetDialogToken(loadData.Content.View.ToString());

            var newContent = UIIoCContainer.GetInstance<IDataContextHolder>(dialogToken);

            if (newContent.DataContext is BaseViewModel)
            {
                var viewModel = newContent.DataContext as BaseViewModel;

                if (viewModel.Initialize(loadData.Content.Parameters)) return;
                viewModel.CallBack = loadData.Content.Callback;
                viewModel.ShownAsDialog = true;
            }

            if (RealMainWindow.OverheadContent.Content != null)
            {
                if (!(RealMainWindow.OverheadContent.Content is IDataContextHolder))
                    throw new Exception("The presented content must implement IDataContextHolder");
                _contents.Push(RealMainWindow.OverheadContent.Content as IDataContextHolder);
            }

            _contents.Push(newContent);
            RealMainWindow.OverheadContent.Content = newContent;
        }

        public void DisplayView(NotificationMessage<ViewLoadData<ViewsEnum>> loadData)
        {
            if (loadData == null || loadData.Content == null ||
                loadData.Notification != MessengerHelper.DisplayViewToken)
                return;

            //Remove all dialogs or previous views from memory
            _contents.Clear();

            var newContent =
                UIIoCContainer.IsRegistered<IDataContextHolder>(loadData.Content.View.ToString()) ?
                UIIoCContainer.GetInstance<IDataContextHolder>(loadData.Content.View.ToString()) :
                (IDataContextHolder)UIIoCContainer.GetInstance(Assembly.GetAssembly(typeof(App)).GetTypes().Single(x => x.Name == loadData.Content.View.ToString()), GetWindowToken(loadData.Content.View.ToString()));

            if (newContent.DataContext is BaseViewModel)
            {
                var viewModel = newContent.DataContext as BaseViewModel;
                if (viewModel.Initialize(loadData.Content.Parameters)) return;
            }
            if (newContent is Window) (newContent as Window).Show();
            else RealMainWindow.SetNewOverheadContent(loadData.Content.View, newContent);

        }

        public void CloseView(NotificationMessage<ViewShutdownData> closeData)
        {
            throw new Exception("Under this UI, views won't be needing to use " +
                                "the close method. The user will be the one deciding " +
                                "what will be shown by clicking the sections on top.");
        }

        public void CloseDialogView(NotificationMessage<DialogShutdownData> closeData)
        {
            if (closeData == null || closeData.Content == null ||
                   closeData.Notification != MessengerHelper.CloseDialogViewToken)
                return;

            //Under this architecture, the stack should contain at least two items, 
            //the base view that called the dialog and the dialog being shown.

            //But if the architecture was changed and cases where there was not an initial 
            //content (RealMainWindow.OverheadContent.Content ==null on DisplayDialog) 
            //could happen. In those scenarios, the stack should contain one element.
            Debug.Assert(_contents.Count > 0);

            var previousContent = _contents.Pop();
            var newContent = _contents.Count == 0 ? null : _contents.Pop();
            Debug.Assert(previousContent.DataContext == closeData.Content.ViewModel);
            Debug.Assert(previousContent == RealMainWindow.OverheadContent.Content);

            RealMainWindow.OverheadContent.Content = newContent;

            if (closeData.Content.ViewModel.CallBack != null)
                closeData.Content.ViewModel.CallBack(closeData.Content.Result);
        }

        public void CloseWindow(NotificationMessage<WindowShutdownData> closeData)

        {
            if (closeData?.Content == null || closeData.Notification != MessengerHelper.CloseWindowToken
                || !(closeData.Content.ViewModel?.VisualTree is Window))
            {
                return;
            }

            closeData.Content.ViewModel.CallBack?.Invoke(closeData.Content.Result);

            ((Window)closeData.Content.ViewModel.VisualTree).Close();
        }


        #endregion
    }
}
