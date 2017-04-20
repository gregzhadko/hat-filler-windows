using GalaSoft.MvvmLight.Command;
using HatDesktop.Model;
using HatDesktop.Properties;
using HatDesktop.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using ViewModelBase = GalaSoft.MvvmLight.ViewModelBase;

namespace HatDesktop.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IPackService _packService;

        private RelayCommand _addCommand;
        private ObservableCollection<PhraseItem> _filtertedPhrases;
        private bool _isBusy;

        private int? _newComplexity;

        private string _newDescription;

        private string _newPhrase;

        private PhraseItem _oldEditValue;
        private ObservableCollection<Pack> _packs;
        private string _packStat;
        private ObservableCollection<PhraseItem> _phrases;
        private ObservableCollection<Tuple<int, string>> _ports;
        private RelayCommand _renamePackCommand;
        private string _selectedAuthor = Settings.Default.SelectedAuthor;
        private int _selectedIndex = -1;
        private Pack _selectedPack;
        private Tuple<int, string> _selectedPort;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IPackService packService)
        {
            _packService = packService;
            Ports = new ObservableCollection<Tuple<int, string>>(_packService.GetPorts());
            SelectedPort = Ports[0];
            ReloadPacks(SelectedPort);
            SelectedIndex = 0;
        }

        public static string[] Authors => Reviewer.DefaultReviewers;

        public Pack SelectedPack
        {
            get { return _selectedPack; }
            set
            {
                if (value == _selectedPack) return;
                _selectedPack = value;
                RaisePropertyChanged();
                Phrases = new ObservableCollection<PhraseItem>(_selectedPack?.Phrases ?? new List<PhraseItem>());
                NewDescription = "";
                NewPhrase = "";
                NewComplexity = 1;
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value == _selectedIndex) return;
                _selectedIndex = value;
                RaisePropertyChanged();
                if (_selectedIndex >= 0)
                    SelectedPack = LoadPackData(Packs[_selectedIndex], SelectedPort.Item1);
            }
        }

        public ObservableCollection<PhraseItem> Phrases
        {
            get { return _phrases; }
            private set
            {
                Set(ref _phrases, value);
                FiltertedPhrases = value;
            }
        }

        public ObservableCollection<PhraseItem> FiltertedPhrases
        {
            get { return _filtertedPhrases; }
            set { Set(ref _filtertedPhrases, value); }
        }

        public ObservableCollection<Pack> Packs
        {
            get { return _packs; }
            set { Set(ref _packs, value); }
        }

        public ObservableCollection<Tuple<int, string>> Ports
        {
            get { return _ports; }
            set { Set(ref _ports, value); }
        }

        public Tuple<int, string> SelectedPort
        {
            get { return _selectedPort; }
            set { Set(ref _selectedPort, value); }
        }

        public RelayCommand AddCommand => _addCommand ?? (_addCommand = new RelayCommand(() =>
        {
            IsBusy = true;
            var phrase = GetNewPhrase(out string error);
            if (phrase == null)
            {
                MessageBox.Show(error);
                IsBusy = false;
                return;
            }

            _packService.AddPhrase(SelectedPack.Id, phrase, out error);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show($"The new word wasn't added to the pack:\n{error}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                IsBusy = false;
                return;
            }
            ReloadSelectedPackData();

            NewPhrase = "";
            NewDescription = "";
            NewComplexity = null;
            IsBusy = false;
        }));

        public string NewPhrase
        {
            get { return _newPhrase; }
            set
            {
                Set(ref _newPhrase, value);
                ApplyFilter();
            }
        }

        public int? NewComplexity
        {
            get { return _newComplexity; }
            set { Set(ref _newComplexity, value); }
        }

        public string NewDescription
        {
            get { return _newDescription; }
            set { Set(ref _newDescription, value); }
        }

        public RelayCommand<GridViewBeginningEditRoutedEventArgs> BeginEditCommand
            => new RelayCommand<GridViewBeginningEditRoutedEventArgs>(args =>
            {
                var oldEditValue = args.Row.Item as PhraseItem;
                if (oldEditValue == null)
                    return;

                _oldEditValue = oldEditValue.Clone();
            });

        public RelayCommand<GridViewRowEditEndedEventArgs> EndEditCommand
            => new RelayCommand<GridViewRowEditEndedEventArgs>(arg =>
            {
                if (arg.EditAction == GridViewEditAction.Cancel)
                    return;

                var editedPhrase = arg.NewData as PhraseItem;
                if (editedPhrase == null)
                    return;

                if (string.IsNullOrWhiteSpace(editedPhrase.Phrase) ||
                    string.IsNullOrWhiteSpace(editedPhrase.Description) || editedPhrase.Complexity > 5 ||
                    editedPhrase.Complexity < 1)
                {
                    MessageBox.Show("The phrase wasn't saved. Please fill in all mandatory fields", "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                editedPhrase.Phrase = StringUtils.FormatPhrase(editedPhrase.Phrase);
                editedPhrase.Description = StringUtils.FormatDescription(editedPhrase.Description);

                if (!IsNeedToUpdatePhrase(_oldEditValue, editedPhrase))
                {
                    return;
                }

                IsBusy = true;
                editedPhrase.UpdateAuthor(SelectedAuthor);
                _packService.EditPhrase(SelectedPack.Id, _oldEditValue, editedPhrase, SelectedAuthor, out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show($"The new word wasn't edited in the pack:\n{error}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    ReloadSelectedPackData();
                    IsBusy = false;
                    return;
                }

                IsBusy = false;
            });

        private static bool IsNeedToUpdatePhrase(PhraseItem oldPhrase, PhraseItem newPhrase)
        {
            return oldPhrase.Phrase != newPhrase.Phrase || Math.Abs(oldPhrase.Complexity - newPhrase.Complexity) > 0.1 ||
                   oldPhrase.Description != newPhrase.Description;
        }

        public RelayCommand<GridViewDeletingEventArgs> DeletingCommand
            => new RelayCommand<GridViewDeletingEventArgs>(args =>
            {
                foreach (var item in args.Items.OfType<PhraseItem>())
                {
                    _packService.DeletePhrase(SelectedPack.Id, item.Phrase, out string error);
                    if (!string.IsNullOrEmpty(error))
                        MessageBox.Show($"The word wasn't deleted:\n{error}", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                }
            });

        public RelayCommand FormatAllCommand => new RelayCommand(() =>
        {
            foreach (var phraseItem in Phrases)
            {
                var oldPhrase = phraseItem.Clone();
                var newPhrase = StringUtils.FormatPhrase(phraseItem.Phrase);
                var newDescription = StringUtils.FormatDescription(phraseItem.Description);

                if (newPhrase == phraseItem.Phrase && newDescription == phraseItem.Description)
                    continue;

                phraseItem.Phrase = newPhrase;
                phraseItem.Description = newDescription;

                phraseItem.UpdateAuthor(SelectedAuthor);
                _packService.EditPhrase(SelectedPack.Id, oldPhrase, phraseItem, SelectedAuthor, out string error);
                if (!string.IsNullOrEmpty(error))
                    MessageBox.Show($"The word wasn't edited in the pack:\n{error}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        });

        public string PackStat
        {
            get { return _packStat; }
            set { Set(ref _packStat, value); }
        }

        public RelayCommand RenamePackCommand => _renamePackCommand ?? (_renamePackCommand = new RelayCommand(() =>
        {
            var renamePackViewModel = new RenamePackViewModel(SelectedPack);
            var dialog = new RenamePackView { DataContext = renamePackViewModel };
            var result = dialog.ShowDialog();
            var selectedIndex = SelectedIndex;
            if (result.HasValue && result.Value)
            {
                _packService.EditPack(SelectedPack.Id, renamePackViewModel.Name,
    renamePackViewModel.Description, out string error);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    MessageBox.Show($"The pack wasn't updated:\n{error}");
                    return;
                }

                ReloadPacks(SelectedPort);
                SelectedIndex = selectedIndex;
            }
        }));

        public string SelectedAuthor
        {
            get { return _selectedAuthor; }
            set
            {
                Set(ref _selectedAuthor, value);
                Settings.Default.SelectedAuthor = value;
                Settings.Default.Save();
                ReloadSelectedPackData();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public string[] ReviewerNames => Reviewer.DefaultReviewers;

        public bool ReviewPhrase(PhraseItem phrase, State state)
        {
            _packService.ReviewPhrase(SelectedPack.Id, phrase, SelectedAuthor, state, out string error);
            var isSuccess = string.IsNullOrEmpty(error);
            if (isSuccess)
            {
                SelectedPack = LoadPackData(SelectedPack, SelectedPort.Item1);
            }
            return isSuccess;
        }

        private PhraseItem GetNewPhrase(out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(NewPhrase) || string.IsNullOrWhiteSpace(NewDescription))
            {
                error = "Phrase and description cannot be empty";
                return null;
            }

            var correctedPhrase = StringUtils.FormatPhrase(NewPhrase);
            if (!IsPhraseValid(correctedPhrase))
            {
                error = "The phrase already exists!";
                return null;
            }

            var phrase = new PhraseItem
            {
                Phrase = correctedPhrase,
                Complexity = NewComplexity ?? 1,
                Description = StringUtils.FormatDescription(NewDescription)
            };
            phrase.UpdateAuthor(SelectedAuthor);

            return phrase;
        }

        private void ApplyFilter()
        {
            var phrase = NewPhrase.Trim();
            FiltertedPhrases = string.IsNullOrWhiteSpace(phrase)
                ? Phrases
                : new ObservableCollection<PhraseItem>(
                    Phrases.Where(p => p.Phrase.IndexOf(phrase, 0, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private bool IsPhraseValid(string phrase)
            => !Phrases.Select(p => p.Phrase.Trim().ToLowerInvariant()).Contains(phrase.ToLowerInvariant());

        private void ReloadPacks(Tuple<int, string> port)
        {
            var packs = _packService.GetAllPacksInfo(port.Item1, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                var errorMessage = "Error occurs on packs reloading:\n" + error;
                Logger.Error(errorMessage);
                MessageBox.Show(errorMessage);
                return;
            }

            Packs = new ObservableCollection<Pack>(packs.OrderBy(p => p.Id));
        }

        private Pack LoadPackData(Pack pack, int port)
        {
            pack = _packService.GetPackById(port, pack.Id, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show("Error occurs on pack reloading:\n" + error);
                return null;
            }

            Phrases = new ObservableCollection<PhraseItem>(pack.Phrases);
            CalculateStat(pack);

            return pack;
        }

        private void ReloadSelectedPackData()
        {
            SelectedPack = LoadPackData(SelectedPack, SelectedPort.Item1);
        }

        private void CalculateStat(Pack pack)
            =>
                PackStat =
                    $"Average complexity: {pack.Phrases.Sum(phrase => phrase.Complexity) / pack.Phrases.Count:0.###}";
    }
}