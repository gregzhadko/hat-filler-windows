using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HatNewUI.Handlers;
using Model;

namespace HatNewUI.ViewModel
{
    public class FillerViewModel : GridBasedViewModel<PhraseItem>
    {
        private PackService _service;
        private Pack _selectedPack;
        private ObservableCollection<Pack> _packs;
        private int _defaultPackId = 1;
        private bool _isInitializing;

        protected override void Init(params object[] parameters)
        {
            _service = new PackService();
            InitialLoad();
        }

        private async void InitialLoad()
        {
            _isInitializing = true;
            var packs = _service.GetAllPackInfoAsync();
            var selectedPack = _service.GetPackByIdAsync(_defaultPackId);
            await Task.WhenAll(packs, selectedPack);
            Packs = new ObservableCollection<Pack>(packs.Result.OrderBy(p => p.Id));
            SelectedPack = selectedPack.Result;
            _isInitializing = false;
        }

        protected override void CommitNewItem()
        {
            SelectedPack.Phrases.Add(SelectedItem);
            try
            {
                _service.AddPhrase(SelectedPack.Id, SelectedItem);
            }
            catch (Exception ex)
            {
                NotificationHandler.Show(ex.Message, "Error");
            }
        }

        protected override void UpdateItem()
        {
            try
            {
                //TODO: fix author
                _service.EditPhrase(SelectedPack.Id, BackupItem, SelectedItem, "zhadko");
            }
            catch (Exception ex)
            {
                NotificationHandler.Show(ex.Message, "Error");
            }
        }

        protected override bool DeleteItem()
        {
            try
            {
                _service.DeletePhrase(SelectedPack.Id, SelectedItem.Phrase);
                return true;
            }
            catch (Exception ex)
            {
                NotificationHandler.Show(ex.Message, "Error");
            }
            return false;
        }

        protected override void LoadItems()
        {
            Items = new ObservableCollection<PhraseItem>(SelectedPack.Phrases);
        }

        public Pack SelectedPack
        {
            get => _selectedPack;
            set
            {
                Set(ref _selectedPack, value);
                if (!_isInitializing)
                {
                    _selectedPack = _service.GetPackById(_selectedPack.Id);
                }
                LoadItems();
            }
        }

        public ObservableCollection<Pack> Packs
        {
            get => _packs;
            set => Set(ref _packs, value);
        }

    }
}
