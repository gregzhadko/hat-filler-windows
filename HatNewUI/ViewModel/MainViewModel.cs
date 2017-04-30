using System.Windows;
using GalaSoft.MvvmLight.Command;
using MVVMBase;
using HatNewUI.Helpers;

namespace HatNewUI.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            //CallBack = x => App.LoadLoginWindow();
        }
        protected override void Init(params object[] parameters)
        {
            base.Init(parameters);
        }



        public string DebugNotification
        {
            get
            {
                return
#if DEBUG
 "[DEBUG]"
#else
  string.Empty
#endif
;
            }
        }



        private object _overheadContent;
        public object OverheadContent
        {
            get { return _overheadContent; }
            set { Set(ref _overheadContent, value); }
        }


        private object _principalContainer;
        public object PrincipalContainer
        {
            get { return _principalContainer; }
            set { Set(ref _principalContainer, value); }
        }


        private bool _initializingSection;
        public bool InitializingSection
        {
            get { return _initializingSection; }
            set { Set(ref _initializingSection, value); }
        }


        private ViewsEnum? _currentSection;
        public ViewsEnum? CurrentSection
        {
            get { return _currentSection; }
            set { Set(ref _currentSection, value); }
        }


        private RelayCommand<ViewsEnum> _showView;
        public RelayCommand<ViewsEnum> ShowView
        {
            get
            {
                return _showView ?? (_showView = new RelayCommand<ViewsEnum>(
                    p =>
                    {
                        try
                        {
                            InitializingSection = true;
                            DisplayView(p);
                        }
                        finally
                        {
                            InitializingSection = false;
                        }
                    },
                    p =>
                    {
                        return true;
                    }));
            }
        }


    }
}
