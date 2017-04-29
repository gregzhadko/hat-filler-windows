using System;

namespace HatNewUI.UtilsObject
{
    public class DialogLoadData<TView> : ViewLoadData<TView>
    {
        public Action<DialogResult> Callback { get; private set; }

        public DialogLoadData(TView view, Action<DialogResult> callback,
            params object[] parameters)
            : base(view, parameters)
        {
            Callback = callback;
        }

    }
}
