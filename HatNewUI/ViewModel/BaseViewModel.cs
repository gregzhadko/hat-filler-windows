using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using HatNewUI.Helpers;
using HatNewUI.UtilsObject;

namespace HatNewUI.ViewModel
{
    public abstract class BaseViewModel : ViewModelBase
    {
        private bool? _shownAsDialog;

        public bool? ShownAsDialog
        {
            get => _shownAsDialog;
            set => Set(ref _shownAsDialog, value);
        }


        private object _visualTree;
        public object VisualTree
        {
            get => _visualTree;
            set => Set(ref _visualTree, value);
        }

        public static void AsyncAction(Action action,
            Action<Task> callback = null, bool callbackOnUIThread = true)
        {
            Util.AsyncAction(action, callback, callbackOnUIThread);
        }

        public static void AsyncAction<T>(Func<T> action,
            Action<Task<T>> callback = null, bool callbackOnUIThread = true)
        {
            Util.AsyncAction(action, callback, callbackOnUIThread);
        }

        //protected void DisplayView<T>(T view)
        //{
        //    DisplayView(view, null);
        //}

        //protected void DisplayDialog<T>(T view, Action<DialogResult> callback)
        //{
        //    DisplayDialog(view, callback, null);
        //}

        protected void DisplayView<T>(T view, params object[] parameters)
        {
            if (ShownAsDialog.HasValue && ShownAsDialog.Value)
                throw new Exception("A dialog cannot directly show a view.");
            MessengerHelper.SendDisplayViewMessage(this, view, parameters);
        }

        protected void DisplayDialog<T>(T view,
            Action<DialogResult> callback, params object[] parameters)
        {
            MessengerHelper.SendDisplayDialogMessage(this, view, callback, parameters);
        }

        protected void Close(DialogResult res = null)
        {
            MessengerHelper.SendCloseMessage(this, res);
        }

        protected void CloseWindow(DialogResult res = null)
        {
            MessengerHelper.SendCloseWindowMessage(this, res);
        }

        protected bool CancelInit { get; set; }
        protected virtual void Init(params object[] parameters) { }

        [DebuggerStepThrough]
        public bool Initialize(params object[] parameters)
        {
            CancelInit = false;
            Init(parameters);
            return CancelInit;
        }
        public Action<DialogResult> CallBack { get; set; }


    }


}
