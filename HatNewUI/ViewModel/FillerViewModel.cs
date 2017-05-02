using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HatNewUI.Handlers;
using Model;

namespace HatNewUI.ViewModel
{
    public class FillerViewModel : GridBasedViewModel<PhraseItem>
    {
        private PackService _service;
        private Pack _selectedPack;

        protected override void Init(params object[] parameters)
        {
            _service = new PackService();
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
            //TODO: fix pack id
            SelectedPack = _service.GetPackById(1);
            Items = new ObservableCollection<PhraseItem>(SelectedPack.Phrases);
        }

        public Pack SelectedPack
        {
            get { return _selectedPack; }
            set { Set(ref _selectedPack, value); }
        }
    }
}
