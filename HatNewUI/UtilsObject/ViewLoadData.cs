namespace HatNewUI.UtilsObject
{
    public class ViewLoadData<TView>
    {
        public ViewLoadData(TView view, params object[] parameters)
        {
            View = view;
            Parameters = parameters;
        }

        public TView View { get; set; }
        public object[] Parameters { get; protected set; }
    }
}
