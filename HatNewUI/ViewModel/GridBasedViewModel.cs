using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using HatNewUI.Handlers;
using HatNewUI.Helpers;
using MahApps.Metro.Controls;
using Model;

namespace HatNewUI.ViewModel
{
    public abstract class GridBasedViewModel<T> : GridBasedViewModel
        where T : PhraseItem, ICloneable, new()
    {
        private ObservableCollection<T> _items;

        private T _selectedItem;

        public ObservableCollection<T> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }


        protected T BackupItem { get; set; }

        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                SelectedItemChanged();
                Set(ref _selectedItem, value);
            }
        }


        protected override bool HasSelectedItem
        {
            get => SelectedItem != null;
            set
            {
                if (!value)
                {
                    SelectedItem = null;
                }
            }
        }

        protected override bool IsCurrentItemNew => SelectedItem != null && SelectedItem.IsNew;

        protected override bool IsSelectedItemValid => SelectedItem != null && SelectedItem.IsValid;

        protected override string SelectedItemValidationErrors => SelectedItem == null ? string.Empty : SelectedItem.Error;
        protected override int ItemCount => Items.IsNullOrEmpty() ? 0 : Items.Count;

        protected virtual T GetNewItem()
        {
            return new T();
        }

        protected override void InsertNewItem()
        {
            PreEditing();
            var newItem = GetNewItem();
            Items.Insert(0, newItem);
            //DataGrid.Items.Add(newItem);
            DataGrid.ScrollIntoView(newItem);
            DataGrid.UpdateLayout();
            DataGrid.FocusRow(0);
            StartEditingMode();
        }

        protected override void RollbackNewItem()
        {
            Items.Remove(SelectedItem);
        }

        protected override void CreateBackupItem()
        {
            //TODO: Делать deep copy через статью описанную на хабре
            BackupItem = (T) SelectedItem.Clone(); //DeepCopy();
        }

        protected override void ClearBackupItem()
        {
            BackupItem = null;
        }

        protected override void RestoreBackedUpItem()
        {
            var selIndex = SelectedIndex;
            Items.RemoveAt(selIndex);
            Items.Insert(selIndex, BackupItem);
            ClearBackupItem();
            DataGrid.FocusRow(selIndex);
        }

        protected override void SelectedItemChanged()
        {

        }
    }


    public abstract class GridBasedViewModel : BaseViewModel
    {
        private static readonly Func<KeyEventArgs, bool> ShouldTunnelDownEvent = p =>
        {
            /*
             * Don't take into consideration calls originated from:
             *  - The Wing Grid on the Facilities GridBasedView.
             *  - The ChecklistItem Grid on the Pathways GridBasedView.
             */
            var source = p.OriginalSource as DependencyObject;

            if (source == null)
            {
                return false;
            }
            var dg = source.FindParentPanel<DataGrid>();
            return dg != null && !string.IsNullOrWhiteSpace(AttachedProperties.GetTag(dg));
        };


        private RelayCommand<DataGrid> _add;

        private string _addButtonText = "Add";


        private RelayCommand<DataGridBeginningEditEventArgs> _beginningEdit;

        private bool _canBeAdded = true;

        private bool _canBeDeleted = true;


        private RelayCommand _cancel;

        private RelayCommand<RoutedEventArgs> _creatingDataGrid;

        private RelayCommand _delete;


        private RelayCommand _edit;

        private RelayCommand<DataGridCellEditEndingEventArgs> _endingEdit;


        private RelayCommand _help;


        private bool _isEditable = true;


        private bool _isEditing;

        private bool _isEnterKeyDisabled;

        private RelayCommand<KeyEventArgs> _keyDown;


        private RelayCommand<MouseButtonEventArgs> _mouseDoubleClicked;


        private RelayCommand _refresh;

        private RelayCommand<DataGridRowEditEndingEventArgs> _rowEditEnding;

        private int _selectedIndex;


        private Action _showHelp;
        protected DataGrid DataGrid;
        protected bool ReloadOnlyEditedItems = false;
        protected abstract bool HasSelectedItem { get; set; }
        protected abstract int ItemCount { get; }
        protected abstract bool IsSelectedItemValid { get; }
        protected abstract string SelectedItemValidationErrors { get; }
        protected abstract bool IsCurrentItemNew { get; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }

        public bool IsEditable
        {
            get => _isEditable;
            set => Set(ref _isEditable);
        }

        public virtual bool CanBeAdded
        {
            get => _canBeAdded;
            set => Set(ref _canBeAdded, value);
        }

        public bool CanBeDeleted
        {
            get => _canBeDeleted;
            set => Set(ref _canBeDeleted, value);
        }

        public string AddButtonText
        {
            get => _addButtonText;
            set => Set(ref _addButtonText, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                AddButtonText = value ? "Save" : "Add";
                Set(ref _isEditing, value);
            }
        }

        public bool IsEnterKeyDisabled
        {
            get => _isEnterKeyDisabled;
            set => Set(ref _isEnterKeyDisabled, value);
        }

        public RelayCommand<RoutedEventArgs> CreatingDataGrid
        {
            get
            {
                return _creatingDataGrid ?? (_creatingDataGrid = new RelayCommand<RoutedEventArgs>(
                           p =>
                           {
                               DataGrid = p.Source as DataGrid;
                               FocusFirstCell();
                           }));
            }
        }

        public RelayCommand<DataGridBeginningEditEventArgs> BeginningEdit
        {
            get { return _beginningEdit ?? (_beginningEdit = new RelayCommand<DataGridBeginningEditEventArgs>(p => { })); }
        }

        public RelayCommand<DataGridRowEditEndingEventArgs> RowEditEnding
        {
            get
            {
                return _rowEditEnding ?? (_rowEditEnding = new RelayCommand<DataGridRowEditEndingEventArgs>(
                           p => { }));
            }
        }

        public RelayCommand<DataGridCellEditEndingEventArgs> EndingEdit
        {
            get
            {
                return _endingEdit ?? (_endingEdit = new RelayCommand<DataGridCellEditEndingEventArgs>(
                           p =>
                           {
                               if (p.EditAction != DataGridEditAction.Commit)
                               {
                                   return;
                               }
                               if (IsEditing)
                               {
                                   p.Cancel = true;
                               }
                           }));
            }
        }

        public RelayCommand<KeyEventArgs> KeyDown => _keyDown ?? (_keyDown = new RelayCommand<KeyEventArgs>(KeyDownAction));

        public virtual RelayCommand<DataGrid> Add
        {
            get
            {
                return _add ?? (_add = new RelayCommand<DataGrid>(
                           AddAction,
                           dataGrid => IsEditing || IsEditable && CanBeAdded));
            }
        }

        public virtual RelayCommand<MouseButtonEventArgs> MouseDoubleClicked
        {
            get
            {
                return _mouseDoubleClicked ?? (_mouseDoubleClicked = new RelayCommand<MouseButtonEventArgs>(
                           MouseDoubleClickAction,
                           p => CanStartEdit()));
            }
        }

        public RelayCommand Help
        {
            get
            {
                return _help ?? (_help = new RelayCommand(
                           () => ShowHelp(),
                           () => ShowHelp != null));
            }
        }

        /// <summary>
        ///     This is the property that describes what should be done when the ?(HELP) button is clicked.
        ///     Implementing classes must set a value to this Action for the button to become available.
        /// </summary>
        protected Action ShowHelp
        {
            get => _showHelp;
            set
            {
                _showHelp = value;
                Help.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand Refresh
        {
            get { return _refresh ?? (_refresh = new RelayCommand(RefreshAction, () => !IsEditing && IsEditable)); }
        }

        public RelayCommand Cancel
        {
            get { return _cancel ?? (_cancel = new RelayCommand(CancelEdit, () => IsEditing)); }
        }

        public RelayCommand Delete
        {
            get
            {
                return _delete ?? (_delete = new RelayCommand(
                           () =>
                           {
                               if (DeleteItem())
                               {
                                   LoadItems();
                               }
                           },
                           () => !IsEditing && IsEditable && CanBeDeleted));
            }
        }

        public RelayCommand Edit
        {
            get
            {
                return _edit ?? (_edit = new RelayCommand(
                           () => StartEditingMode(),
                           () => !IsEditing && IsEditable));
            }
        }

        protected override void Init(params object[] parameters)
        {
            base.Init(parameters);
            LoadItems();
            SelectedIndex = 0;
        }

        protected abstract void InsertNewItem();
        protected abstract void RollbackNewItem();
        protected abstract void CommitNewItem();
        protected abstract void UpdateItem();
        protected abstract bool DeleteItem();
        protected abstract void LoadItems();
        protected abstract void CreateBackupItem();
        protected abstract void ClearBackupItem();
        protected abstract void RestoreBackedUpItem();
        protected abstract void SelectedItemChanged();
        protected abstract void PreAcceptChanges();

        protected virtual bool CanStartEdit()
        {
            return true;
        }

        protected virtual void UpdateChangedItem()
        {
        }

        private void AcceptChanges()
        {
            PreAcceptChanges();
            if (!CheckItemValid())
            {
                return;
            }
            StopEditingMode();
            DataGrid.CommitEdit(DataGridEditingUnit.Row, false);
            if (IsCurrentItemNew)
            {
                CommitNewItem();
                RefreshAction();
            }
            else
            {
                UpdateItem();
                ClearBackupItem();
                if (ReloadOnlyEditedItems)
                {
                    UpdateChangedItem();
                }
                else
                {
                    RefreshAction();
                }
            }
        }

        public void CancelEdit()
        {
            StopEditingMode();

            if (IsCurrentItemNew)
            {
                RollbackNewItem();
            }
            else
            {
                RestoreBackedUpItem();
                DataGrid.CancelEdit(DataGridEditingUnit.Row);
            }
        }

        protected void StartEditingMode(int? columnIndex = null)
        {
            if (!HasSelectedItem)
            {
                return;
            }

            columnIndex = columnIndex ?? DataGrid.GetCurrentColumnIndex();
            Debug.Assert(!IsEditing);
            IsEditing = true;

            if (!IsCurrentItemNew)
            {
                CreateBackupItem();
            }

            var selectedRow = (DataGridRow) DataGrid.ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

            var presenter = LocalUtils.FindVisualChild<DataGridCellsPresenter>(selectedRow);

            if (presenter == null)
            {
                return;
            }

            selectedRow.IsEnabled = true;
            StartEditingColumns(columnIndex, presenter);
        }

        private void StartEditingColumns(int? columnIndex, ItemsControl presenter)
        {
            for (var i = 0; i < DataGrid.Columns.Count; i++)
            {
                var cell = (DataGridCell) presenter.ItemContainerGenerator.ContainerFromIndex(i);

                cell.IsEditing = true;

                if (i != columnIndex)
                {
                    continue;
                }
                DataGrid.UpdateLayout();

                var contentPresenter = LocalUtils.FindVisualChild<ContentPresenter>(cell);
                var frameworkElement = (FrameworkElement) VisualTreeHelper.GetChild(contentPresenter, 0);

                frameworkElement?.Focus();
            }
        }


        private void StopEditingMode()
        {
            var selectedRow = (DataGridRow) DataGrid.ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

            var presenter = LocalUtils.FindVisualChild<DataGridCellsPresenter>(selectedRow);

            if (presenter == null)
            {
                return;
            }
            for (var i = 0; i < DataGrid.Columns.Count; i++)
            {
                var cell = (DataGridCell) presenter.ItemContainerGenerator.ContainerFromIndex(i);
                cell.IsEditing = false;
            }
            IsEditing = false;
        }


        private void MoveNextCell()
        {
            if (DataGrid.CurrentCell == default(DataGridCellInfo) || !IsEditing)
            {
                return;
            }

            var newCell = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
                ? DataGrid.SelectedCells.OrderByDescending(c => c.Column.DisplayIndex)
                    .FirstOrDefault(c => c.Column.DisplayIndex < DataGrid.CurrentCell.Column.DisplayIndex)
                : DataGrid.SelectedCells.OrderBy(c => c.Column.DisplayIndex)
                    .FirstOrDefault(c => c.Column.DisplayIndex > DataGrid.CurrentCell.Column.DisplayIndex);

            if (newCell == default(DataGridCellInfo))
            {
                return;
            }
            DataGrid.CurrentCell = newCell;
            DataGrid.SelectCellByIndex(SelectedIndex, DataGrid.CurrentCell.Column.DisplayIndex);
        }

        protected virtual bool CheckItemValid()
        {
            if (IsSelectedItemValid)
            {
                return true;
            }

            NotificationHandler.Show(
                $"The item does not meet the required validations. Please fix the following errors: \n\n{SelectedItemValidationErrors}", "Warning", MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }


        private void FocusFirstCell()
        {
            try
            {
                if (ItemCount == 0)
                {
                    return;
                }
                if (DataGrid == null)
                {
                    return;
                }
                DataGrid.UpdateLayout();
                var cell = DataGrid.GetCell(0, 0);
                if (cell == null)
                {
                    return;
                }
                cell.Focus();
            }
            catch
            {
                // ignored
            }
        }

        protected virtual void PreEditing()
        {
        }

        private void KeyDownAction(KeyEventArgs eventArgs)
        {
            switch (eventArgs.Key)
            {
                case Key.Tab:
                    eventArgs.Handled = true;
                    MoveNextCell();
                    break;

                case Key.Escape:
                    if (IsEditing)
                    {
                        if (ShouldTunnelDownEvent(eventArgs))
                        {
                            return;
                        }
                        eventArgs.Handled = true;
                        CancelEdit();
                    }
                    break;

                case Key.Return:
                    if (ShouldTunnelDownEvent(eventArgs))
                    {
                        return;
                    }

                    OnReturnKeyPress(eventArgs);
                    break;
            }
        }

        protected virtual void OnReturnKeyPress(KeyEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            if (IsEditing)
            {
                AcceptChanges();
            }
            else
            {
                StartEditingMode();
            }
        }

        protected virtual void AddAction(DataGrid dataGrid)
        {
            if (IsEditing)
            {
                AcceptChanges();
            }
            else
            {
                InsertNewItem();
            }
        }

        protected virtual void MouseDoubleClickAction(MouseButtonEventArgs eventArgs)
        {
            var dataGrid = GetParentDataGrid(eventArgs);
            if (IsEditing)
            {
                Add.Execute(dataGrid);
            }
            else
            {
                Edit.Execute(dataGrid);
            }
        }

        private void RefreshAction()
        {
            HasSelectedItem = false;
            LoadItems();
        }

        protected static DataGrid GetParentDataGrid(RoutedEventArgs p)
        {
            return ((DependencyObject) p.OriginalSource).TryFindParent<DataGrid>();
        }
    }
}