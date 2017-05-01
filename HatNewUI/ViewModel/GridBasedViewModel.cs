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
using Action = System.Action;

namespace HatNewUI.ViewModel
{
    public abstract class GridBasedViewModel<T> : GridBasedViewModel
        where T : PhraseItem, new()
    {

        protected virtual T GetNewItem() { return new T(); }

        private ObservableCollection<T> _items;
        public ObservableCollection<T> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }



        private T BackupItem { get; set; }

        private T _selectedItem;
        public T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                SelectedItemChanged();
                Set(ref _selectedItem, value);
            }
        }


        protected override bool HasSelectedItem
        {
            get { return SelectedItem != null; }
            set { if (!value) SelectedItem = null; }
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
            Debug.Assert(!IsCurrentItemNew);
            Debug.Assert(SelectedItem != null);
            BackupItem = SelectedItem.DeepCopy();
        }
        protected override void ClearBackupItem()
        {
            Debug.Assert(!IsCurrentItemNew);
            Debug.Assert(BackupItem != null);
            BackupItem = null;
        }
        protected override void RestoreBackedUpItem()
        {
            Debug.Assert(!IsCurrentItemNew);
            Debug.Assert(BackupItem != null);
            var selIndex = SelectedIndex;
            Items.RemoveAt(selIndex);
            Items.Insert(selIndex, BackupItem);
            ClearBackupItem();
            DataGrid.FocusRow(selIndex);
        }

        protected override bool IsCurrentItemNew => SelectedItem != null && SelectedItem.IsNew;

        protected override bool IsSelectedItemValid => SelectedItem != null && SelectedItem.IsValid;

        protected override string SelectedItemValidationErrors => SelectedItem == null ? string.Empty : SelectedItem.Error;
        protected override void SelectedItemChanged() { }
        protected override int ItemCount => Items.IsNullOrEmpty() ? 0 : Items.Count;
    }


    public abstract class GridBasedViewModel : BaseViewModel
    {
        protected bool ReloadOnlyEditedItems = false;
        protected DataGrid DataGrid;
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
        protected abstract bool HasSelectedItem { get; set; }
        protected abstract int ItemCount { get; }
        protected abstract bool IsSelectedItemValid { get; }
        protected abstract string SelectedItemValidationErrors { get; }
        protected abstract bool IsCurrentItemNew { get; }
        protected abstract void SelectedItemChanged();
        protected virtual bool CanStartEdit() { return true; }
        protected virtual void UpdateChangedItem() { }

        void AcceptChanges()
        {
            if (!CheckItemValid()) return;
            StopEditingMode();
            DataGrid.CommitEdit(DataGridEditingUnit.Row, false);
            if (IsCurrentItemNew)
            {
                CommitNewItem();
                RefreshAction();
            }
            else
            {
                ClearBackupItem();
                UpdateItem();
                if (ReloadOnlyEditedItems) UpdateChangedItem();
                else RefreshAction();
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
            if (!HasSelectedItem) return;

            columnIndex = columnIndex ?? DataGrid.GetCurrentColumnIndex();
            Debug.Assert(!IsEditing);
            IsEditing = true;

            if (!IsCurrentItemNew) CreateBackupItem();

            var selectedRow = (DataGridRow)DataGrid.ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

            var presenter = LocalUtils.FindVisualChild<DataGridCellsPresenter>(selectedRow);

            if (presenter == null) return;

            StartEditingColumns(columnIndex, presenter);
        }

        private void StartEditingColumns(int? columnIndex, ItemsControl presenter)
        {
            for (var i = 0; i < DataGrid.Columns.Count; i++)
            {
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(i);

                cell.IsEditing = true;

                if (i != columnIndex)
                {
                    continue;
                }
                DataGrid.UpdateLayout();

                var contentPresenter = LocalUtils.FindVisualChild<ContentPresenter>(cell);
                var frameworkElement = (FrameworkElement)VisualTreeHelper.GetChild(contentPresenter, 0);

                frameworkElement?.Focus();
            }
        }


        void StopEditingMode()
        {

            var selectedRow = (DataGridRow)DataGrid.ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

            var presenter = LocalUtils.FindVisualChild<DataGridCellsPresenter>(selectedRow);

            if (presenter == null) return;
            for (var i = 0; i < DataGrid.Columns.Count; i++)
            {
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(i);
                cell.IsEditing = false;
            }
            IsEditing = false;
        }


        void MoveNextCell()
        {
            if (DataGrid.CurrentCell == default(DataGridCellInfo) || !IsEditing) return;

            var newCell = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
                ? DataGrid.SelectedCells.OrderByDescending(c => c.Column.DisplayIndex)
                    .FirstOrDefault(c => c.Column.DisplayIndex < DataGrid.CurrentCell.Column.DisplayIndex)
                : DataGrid.SelectedCells.OrderBy(c => c.Column.DisplayIndex)
                    .FirstOrDefault(c => c.Column.DisplayIndex > DataGrid.CurrentCell.Column.DisplayIndex);

            if (newCell == default(DataGridCellInfo)) return;
            DataGrid.CurrentCell = newCell;
            DataGrid.SelectCellByIndex(SelectedIndex, DataGrid.CurrentCell.Column.DisplayIndex);
        }

        protected virtual bool CheckItemValid()
        {
            if (IsSelectedItemValid) return true;

            NotificationHandler.Show(
                $"The item does not meet the required validations. Please fix the following errors: \n\n{SelectedItemValidationErrors}", "Warning", MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }


        private void FocusFirstCell()
        {
            try
            {
                if (ItemCount == 0) return;
                if (DataGrid == null) return;
                DataGrid.UpdateLayout();
                var cell = DataGrid.GetCell(0, 0);
                if (cell == null) return;
                cell.Focus();
            }
            catch
            {
                // ignored
            }
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { Set(ref _selectedIndex, value); }
        }



        private bool _isEditable = true;

        public bool IsEditable
        {
            get { return _isEditable; }
            set { Set(ref _isEditable); }
        }

        private bool _canBeAdded = true;

        public virtual bool CanBeAdded
        {
            get { return _canBeAdded; }
            set { Set(ref _canBeAdded, value); }
        }

        private bool _canBeDeleted = true;

        public bool CanBeDeleted
        {
            get { return _canBeDeleted; }
            set { Set(ref _canBeDeleted, value); }
        }

        private string _addButtonText = "Add";
        public string AddButtonText
        {
            get { return _addButtonText; }
            set { Set(ref _addButtonText, value); }
        }


        private bool _isEditing;
        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                AddButtonText = value ? "Save" : "Add";
                Set(ref _isEditing, value);
            }
        }

        protected virtual void PreEditing()
        {
        }

        private bool _isEnterKeyDisabled;
        public bool IsEnterKeyDisabled
        {
            get { return _isEnterKeyDisabled; }
            set { Set(ref _isEnterKeyDisabled, value); }
        }

        private RelayCommand<RoutedEventArgs> _creatingDataGrid;
        public RelayCommand<RoutedEventArgs> CreatingDataGrid
        {
            get
            {
                return _creatingDataGrid ?? (_creatingDataGrid = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        DataGrid = (p.Source as DataGrid);
                        FocusFirstCell();
                    }));
            }
        }


        private RelayCommand<DataGridBeginningEditEventArgs> _beginningEdit;
        public RelayCommand<DataGridBeginningEditEventArgs> BeginningEdit
        {
            get
            {
                return _beginningEdit ?? (_beginningEdit = new RelayCommand<DataGridBeginningEditEventArgs>(p =>
                {
                }));
            }
        }

        private RelayCommand<DataGridRowEditEndingEventArgs> _rowEditEnding;
        public RelayCommand<DataGridRowEditEndingEventArgs> RowEditEnding
        {
            get
            {
                return _rowEditEnding ?? (_rowEditEnding = new RelayCommand<DataGridRowEditEndingEventArgs>(
                    p =>
                    {

                    }));
            }
        }

        private RelayCommand<DataGridCellEditEndingEventArgs> _endingEdit;
        public RelayCommand<DataGridCellEditEndingEventArgs> EndingEdit
        {
            get
            {
                return _endingEdit ?? (_endingEdit = new RelayCommand<DataGridCellEditEndingEventArgs>(
                    p =>
                    {
                        if (p.EditAction != DataGridEditAction.Commit) return;
                        if (IsEditing) p.Cancel = true;
                    }));
            }
        }

        private static readonly Func<KeyEventArgs, bool> _shouldTunnelDownEvent = p =>
        {
            /*
             * Don't take into consideration calls originated from:
             *  - The Wing Grid on the Facilities GridBasedView.
             *  - The ChecklistItem Grid on the Pathways GridBasedView.
             */
            var source = p.OriginalSource as DependencyObject;

            if (source == null) return false;
            var dg = source.FindParentPanel<DataGrid>();
            return dg != null && !string.IsNullOrWhiteSpace(AttachedProperties.GetTag(dg));
        };

        private RelayCommand<KeyEventArgs> _keyDown;
        public RelayCommand<KeyEventArgs> KeyDown => _keyDown ?? (_keyDown = new RelayCommand<KeyEventArgs>(KeyDownAction));

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
                        if (_shouldTunnelDownEvent(eventArgs))
                        {
                            return;
                        }
                        eventArgs.Handled = true;
                        CancelEdit();
                    }
                    break;

                case Key.Return:
                    if (_shouldTunnelDownEvent(eventArgs))
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


        private RelayCommand<DataGrid> _add;

        public virtual RelayCommand<DataGrid> Add
        {
            get
            {
                return _add ?? (_add = new RelayCommand<DataGrid>(
                    AddAction,
                    dataGrid => IsEditing || (IsEditable && CanBeAdded)));
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


        private RelayCommand<MouseButtonEventArgs> _mouseDoubleClicked;

        public virtual RelayCommand<MouseButtonEventArgs> MouseDoubleClicked
        {
            get
            {
                return _mouseDoubleClicked ?? (_mouseDoubleClicked = new RelayCommand<MouseButtonEventArgs>(
                    MouseDoubleClickAction,
                    p => CanStartEdit()));
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


        private RelayCommand _help;
        public RelayCommand Help
        {
            get
            {
                return _help ?? (_help = new RelayCommand(
                    () => ShowHelp(),
                    () => ShowHelp != null));
            }
        }


        private Action _showHelp;
        /// <summary>
        /// This is the property that describes what should be done when the ?(HELP) button is clicked.
        /// Implementing classes must set a value to this Action for the button to become available.
        /// </summary>
        protected Action ShowHelp
        {
            get { return _showHelp; }
            set
            {
                _showHelp = value;
                Help.RaiseCanExecuteChanged();
            }
        }


        private RelayCommand _refresh;
        public RelayCommand Refresh
        {
            get { return _refresh ?? (_refresh = new RelayCommand(RefreshAction, () => !IsEditing && IsEditable)); }
        }

        private void RefreshAction()
        {
            HasSelectedItem = false;
            LoadItems();
        }


        private RelayCommand _cancel;
        public RelayCommand Cancel
        {
            get { return _cancel ?? (_cancel = new RelayCommand(CancelEdit, () => IsEditing)); }
        }

        private RelayCommand _delete;
        public RelayCommand Delete
        {
            get
            {
                return _delete ?? (_delete = new RelayCommand(
                    () =>
                    {
                        if (DeleteItem()) LoadItems();
                    },
                    () => !IsEditing && IsEditable && CanBeDeleted));
            }
        }



        private RelayCommand _edit;
        public RelayCommand Edit
        {
            get
            {
                return _edit ?? (_edit = new RelayCommand(
                    () => StartEditingMode(),
                    () => !IsEditing && IsEditable));
            }
        }

        protected static DataGrid GetParentDataGrid(RoutedEventArgs p)
        {
            return ((DependencyObject)p.OriginalSource).TryFindParent<DataGrid>();
        }
    }



}
