using GalaSoft.MvvmLight;
using Model;

namespace HatDesktop.ViewModels
{
    public class RenamePackViewModel : ViewModelBase
    {
        private string _name;
        private string _description;

        public RenamePackViewModel(Pack selectedPack)
        {
            Name = selectedPack.Name;
            Description = selectedPack.Description;
        }

        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }
    }
}