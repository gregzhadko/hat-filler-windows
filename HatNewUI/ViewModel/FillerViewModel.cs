using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using HatNewUI.Handlers;
using Model;
using System.Net;
using System.IO;

namespace HatNewUI.ViewModel
{
    public class FillerViewModel : GridBasedViewModel<PhraseItem>
    {
        private PackService _service;
        private Pack _selectedPack;
        private ObservableCollection<Pack> _packs;

        private bool _isInitializing;

        private readonly int _defaultPackId = Properties.Settings.Default.SelectedPackId;
        private readonly string _selectedAuthor = Properties.Settings.Default.SelectedAuthor;

        protected override void Init(params object[] parameters)
        {
            _service = new PackService();
            InitialLoad();
        }

        private async void InitialLoad()
        {
            _isInitializing = true;
            var packsTask = _service.GetAllPackInfoAsync();
            var packTask = _service.GetPackByIdAsync(_defaultPackId);
            await Task.WhenAll(packsTask, packTask);
            Packs = new ObservableCollection<Pack>(packsTask.Result.OrderBy(p => p.Id));
            var selectedPack = Packs.First(p => p.Id == packTask.Result.Id);
            selectedPack.Phrases = packTask.Result.Phrases;
            SelectedPack = selectedPack;
            _isInitializing = false;
        }

        protected override void CommitNewItem()
        {
            try
            {
                StringUtils.FormatPhrase(SelectedItem);
                _service.AddPhrase(SelectedPack.Id, SelectedItem);
                SelectedItem.IsNew = false;
                SelectedPack.Phrases.Add(SelectedItem);
                RaisePropertyChanged(nameof(PhraseCount));
            }
            catch (WebException ex)
            {
                var errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                NotificationHandler.Show(errorResponse, "Error");
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
                StringUtils.FormatPhrase(SelectedItem);
                _service.EditPhrase(SelectedPack.Id, BackupItem, SelectedItem, _selectedAuthor);
                RaisePropertyChanged(nameof(PhraseCount));
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
                SelectedPack.Phrases.Remove(SelectedItem);
                RaisePropertyChanged(nameof(PhraseCount));
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
                RaisePropertyChanged(nameof(PhraseCount));
                Properties.Settings.Default.SelectedPackId = _selectedPack.Id;
            }
        }

        public int PhraseCount => SelectedPack?.Phrases.Count ?? 0;

        public ObservableCollection<Pack> Packs
        {
            get => _packs;
            set => Set(ref _packs, value);
        }

        public RelayCommand FormatAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    foreach (var phraseItem in Items)
                    {
                        var oldPhrase = (PhraseItem)phraseItem.Clone();
                        StringUtils.FormatPhrase(phraseItem);
                        _service.EditPhrase(SelectedPack.Id, oldPhrase, phraseItem, _selectedAuthor);
                    }
                    UpdateSelectedPack();
                });
            }
        }

        private void UpdateSelectedPack()
        {
            SelectedPack = SelectedPack;
        }

        protected override PhraseItem GetNewItem()
        {
            return new PhraseItem() {IsNew = true, Complexity = 1};
        }
    }
}
