using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using HatNewUI.Handlers;
using Model;
using System.Net;
using System.IO;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using HatNewUI.Helpers;

namespace HatNewUI.ViewModel
{
    public class FillerViewModel : GridBasedViewModel<PhraseItem>
    {
        private PackService _service;
        private Pack _selectedPack;
        private ObservableCollection<Pack> _packs;

        private bool _isInitializing;

        private readonly int _defaultPackId = Properties.Settings.Default.SelectedPackId;
        private string _selectedAuthor = Properties.Settings.Default.SelectedAuthor;

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
            selectedPack.Description = packTask.Result.Description;
            SelectedPack = selectedPack;
            _isInitializing = false;
        }

        protected override void CommitNewItem()
        {
            try
            {
                _service.AddPhrase(SelectedPack.Id, SelectedItem);
                SelectedItem.IsNew = false;
                SelectedPack.Phrases.Add(SelectedItem);
                RaisePropertyChanged(() => PhraseCount);
            }
            catch (WebException ex)
            {
                var errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                NotificationHandler.Show(errorResponse, "Error");
                RollbackNewItem();
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
                _service.EditPhrase(SelectedPack.Id, BackupItem, SelectedItem, _selectedAuthor);
            }
            catch (WebException ex)
            {
                var errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                NotificationHandler.Show(errorResponse, "Error");
                RollbackEditingItemAfterEdit();
            }
            catch (Exception ex)
            {
                NotificationHandler.Show(ex.Message, "Error");
            }
        }

        protected virtual void RollbackEditingItemAfterEdit()
        {
            _service.AddPhrase(SelectedPack.Id, BackupItem);
            var index = SelectedPack.Phrases.IndexOf(SelectedItem);
            SelectedPack.Phrases[index] = BackupItem;
            DataGrid.CancelEdit();
        }

        protected override bool CheckItemValid()
        {
            if (!base.CheckItemValid())
            {
                return false;
            }

            var count = Items.Count(i => i.Phrase == SelectedItem.Phrase);
            if (count > 1)
            {
                NotificationHandler.Show("The item is in the pack already", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        protected override void PreAcceptChanges()
        {
            SelectedItem.UpdateAuthor(SelectedAuthor);
            StringUtils.FormatPhrase(SelectedItem);
        }

        protected override bool DeleteItem()
        {
            try
            {
                _service.DeletePhrase(SelectedPack.Id, SelectedItem.Phrase, SelectedAuthor);
                SelectedPack.Phrases.Remove(SelectedItem);
                RaisePropertyChanged(() => PhraseCount);
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
            DataGrid.Items.Refresh();
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
                RaisePropertyChanged(() => PhraseCount);
                RaisePropertyChanged(() => Description);
                Properties.Settings.Default.SelectedPackId = _selectedPack.Id;
            }
        }

        public string SelectedAuthor
        {
            get => _selectedAuthor;
            set
            {
                Set(ref _selectedAuthor, value);
                Properties.Settings.Default.SelectedAuthor = _selectedAuthor;
            }
        }

        public ObservableCollection<string> Authors => Reviewer.DefaultReviewers.ToObservableCollection();

        public string Description => SelectedPack?.Description;
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

        public RelayCommand<PhraseItem> AcceptCommand
        {
            get
            {
                return new RelayCommand<PhraseItem>(phrase => UpdateReviewState(phrase, State.Accept), phrase => true);
            }
        }

        public RelayCommand<PhraseItem> RecommendToEdit
        {
            get
            {
                return new RelayCommand<PhraseItem>(phrase => UpdateReviewState(phrase, State.Edit), phrase => true);
            }
        }

        public RelayCommand<PhraseItem> RecommendToDelete
        {
            get
            {
                return new RelayCommand<PhraseItem>(phrase => UpdateReviewState(phrase, State.Delete), phrase => true);
            }
        }

        private void UpdateReviewState(PhraseItem phrase, State reviewState)
        {
            phrase.ReviewerObjects.First(r => r.Author == SelectedAuthor).ReviewState = reviewState;
            _service.ReviewPhrase(SelectedPack.Id, phrase, SelectedAuthor, reviewState);
        }

        private RelayCommand<PhraseItem> GetReviewCommand(State reviewState)
        {
            return new RelayCommand<PhraseItem>(phrase =>
            {
                phrase.ReviewerObjects.First(r => r.Author == SelectedAuthor).ReviewState = reviewState;
                _service.ReviewPhrase(SelectedPack.Id, phrase, SelectedAuthor, reviewState);
            }, phrase => true);
        }

        private void UpdateSelectedPack()
        {
            SelectedPack = SelectedPack;
        }

        protected override PhraseItem GetNewItem()
        {
            return new PhraseItem {IsNew = true, Complexity = 1};
        }
    }
}
