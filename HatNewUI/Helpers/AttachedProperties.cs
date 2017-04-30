using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HatNewUI.ViewModel;

namespace HatNewUI.Helpers
{
    public static class AttachedProperties
    {

        public class DataGridColumnsBehavior
        {
            public static readonly DependencyProperty BindableColumnsProperty =
                DependencyProperty.RegisterAttached("BindableColumns",
                                                    typeof(ObservableCollection<DataGridColumn>),
                                                    typeof(DataGridColumnsBehavior),
                                                    new UIPropertyMetadata(null, BindableColumnsPropertyChanged));
            private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
            {
                DataGrid dataGrid = source as DataGrid;
                ObservableCollection<DataGridColumn> columns = e.NewValue as ObservableCollection<DataGridColumn>;
                dataGrid.Columns.Clear();
                if (columns == null)
                {
                    return;
                }
                foreach (DataGridColumn column in columns)
                {
                    dataGrid.Columns.Add(column);
                }
                columns.CollectionChanged += (sender, e2) =>
                {
                    NotifyCollectionChangedEventArgs ne = e2 as NotifyCollectionChangedEventArgs;
                    if (ne.Action == NotifyCollectionChangedAction.Reset)
                    {
                        dataGrid.Columns.Clear();
                        foreach (DataGridColumn column in ne.NewItems)
                        {
                            dataGrid.Columns.Add(column);
                        }
                    }
                    else if (ne.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (DataGridColumn column in ne.NewItems)
                        {
                            dataGrid.Columns.Add(column);
                        }
                    }
                    else if (ne.Action == NotifyCollectionChangedAction.Move)
                    {
                        dataGrid.Columns.Move(ne.OldStartingIndex, ne.NewStartingIndex);
                    }
                    else if (ne.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (DataGridColumn column in ne.OldItems)
                        {
                            dataGrid.Columns.Remove(column);
                        }
                    }
                    else if (ne.Action == NotifyCollectionChangedAction.Replace)
                    {
                        dataGrid.Columns[ne.NewStartingIndex] = ne.NewItems[0] as DataGridColumn;
                    }
                };
            }
            public static void SetBindableColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
            {
                element.SetValue(BindableColumnsProperty, value);
            }
            public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
            {
                return (ObservableCollection<DataGridColumn>)element.GetValue(BindableColumnsProperty);
            }
        }



        #region


        public static bool GetNewItemAdded(DependencyObject obj)
        {
            return (bool)obj.GetValue(NewItemAddedProperty);
        }

        public static void SetNewItemAdded(DependencyObject obj, bool value)
        {
            obj.SetValue(NewItemAddedProperty, value);
        }

        // Using a DependencyProperty as the backing store for NewItemAdded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewItemAddedProperty =
            DependencyProperty.RegisterAttached("NewItemAdded", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false, null, NewItemAddedChanged));

        private static object NewItemAddedChanged(DependencyObject d, object basevalue)
        {
            if (!(basevalue is bool) || !(bool)basevalue || !(d is DataGrid)) return false;

            var dataGrid = d as DataGrid;
            if (dataGrid.Items.Count > 0)
                dataGrid.StartEditingRow(dataGrid.Items.Count - 1);

            return true;
        }

        #endregion

        /*
         * Property to directly specify the column that should first be focused on an InlineEditDataGrid
         */
        #region SetEditingFocus
        public static bool GetSetEditingFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SetEditingFocusProperty);
        }

        public static void SetSetEditingFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SetEditingFocusProperty, value);
        }

        // Using a DependencyProperty as the backing store for SetEditingFocus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetEditingFocusProperty =
            DependencyProperty.RegisterAttached("SetEditingFocus", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));
        #endregion


        #region BooleanAddedValue
        public static bool GetBooleanAddedValue(DependencyObject obj)
        {
            return (bool)obj.GetValue(BooleanAddedValueProperty);
        }

        public static void SetBooleanAddedValue(DependencyObject obj, bool value)
        {
            obj.SetValue(BooleanAddedValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for BooleanAddedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BooleanAddedValueProperty =
            DependencyProperty.RegisterAttached("BooleanAddedValue", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));
        #endregion

        /*
         * Tag property to add any string value necessary. Utility Property
         */
        #region Tag
        public static string WingGridTag => "WingGrid";
        public static string ChecklistItemGridTag => "ChecklistItemGrid";

        public static string GetTag(DependencyObject obj)
        {
            return (string)obj.GetValue(TagProperty);
        }

        public static void SetTag(DependencyObject obj, string value)
        {
            obj.SetValue(TagProperty, value);
        }

        // Using a DependencyProperty as the backing store for Tag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagProperty =
            DependencyProperty.RegisterAttached("Tag", typeof(string), typeof(AttachedProperties), new PropertyMetadata(null));
        #endregion

        #region CurrentMainWindowSelection

        public static ViewsEnum? GetCurrentMainWindowSection(DependencyObject obj)
        {
            return (ViewsEnum?)obj.GetValue(CurrentMainWindowSectionProperty);
        }

        public static void SetCurrentMainWindowSection(DependencyObject obj, ViewsEnum? value)
        {
            obj.SetValue(CurrentMainWindowSectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for CurrentMainWindowSection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentMainWindowSectionProperty =
            DependencyProperty.RegisterAttached("CurrentMainWindowSection", typeof(ViewsEnum?), typeof(AttachedProperties), new PropertyMetadata(null));

        #endregion

        #region WindowTitleButton Properties


        public static ViewsEnum GetViewToShow(DependencyObject obj)
        {
            return (ViewsEnum)obj.GetValue(ViewToShowProperty);
        }

        public static void SetViewToShow(DependencyObject obj, ViewsEnum value)
        {
            obj.SetValue(ViewToShowProperty, value);
        }

        // Using a DependencyProperty as the backing store for ViewToShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewToShowProperty =
            DependencyProperty.RegisterAttached("ViewToShow", typeof(ViewsEnum), typeof(AttachedProperties), new PropertyMetadata(ViewsEnum.MainWindow));



        public static Visual GetIcon(DependencyObject obj)
        {
            return (Visual)obj.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject obj, Visual value)
        {
            obj.SetValue(IconProperty, value);
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(Visual), typeof(AttachedProperties), new PropertyMetadata(null));


        #endregion

        #region InputDrivenGridDataContext
        public static object GetInputDrivenGridDataContext(DependencyObject obj)
        {
            return (object)obj.GetValue(InputDrivenGridDataContextProperty);
        }

        public static void SetInputDrivenGridDataContext(DependencyObject obj, object value)
        {
            obj.SetValue(InputDrivenGridDataContextProperty, value);
        }

        // Using a DependencyProperty as the backing store for InputDrivenGridDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputDrivenGridDataContextProperty =
            DependencyProperty.RegisterAttached("InputDrivenGridDataContext", typeof(object), typeof(AttachedProperties), new PropertyMetadata(null, InputDrivenGridDataContextChanged));

        private static void InputDrivenGridDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = GetVisualParent(d) as GridBasedView;
            if (grid == null) return;
            grid.GridBasedDataContext = e.NewValue as GridBasedViewModel;
        }
        #endregion

        #region VisualParent

        public static DependencyObject GetVisualParent(DependencyObject obj)
        {
            return (DependencyObject)obj.GetValue(VisualParentProperty);
        }

        public static void SetVisualParent(DependencyObject obj, DependencyObject value)
        {
            obj.SetValue(VisualParentProperty, value);
        }

        // Using a DependencyProperty as the backing store for VisualParent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisualParentProperty =
            DependencyProperty.RegisterAttached("VisualParent", typeof(DependencyObject), typeof(AttachedProperties), new PropertyMetadata(null));


        #endregion

        //#region Binding interaction triggers
        //public static StyleTriggerCollection GetTriggers(DependencyObject obj)
        //{
        //    return (StyleTriggerCollection)obj.GetValue(TriggersProperty);
        //}

        //public static void SetTriggers(DependencyObject obj, StyleTriggerCollection value)
        //{
        //    obj.SetValue(TriggersProperty, value);
        //}

        //public static readonly DependencyProperty TriggersProperty =
        //    DependencyProperty.RegisterAttached("Triggers", typeof(StyleTriggerCollection), typeof(AttachedProperties), new UIPropertyMetadata(null, OnTriggersChanged));

        //static void OnTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var triggers = (StyleTriggerCollection)e.NewValue;
        //    if (triggers == null) return;
        //    triggers.SelectMany(x => x.Actions).OfType<EventBinding>().ToList().ForEach(x => { x.TriggerVisual = d; });
        //    var existingTriggers = Interaction.GetTriggers(d);

        //    foreach (var trigger in triggers)
        //        existingTriggers.Add(trigger);
        //}
        //#endregion

        #region TabItemViewModel


        public static BaseViewModel GetTabItemViewModel(DependencyObject obj)
        {
            return (BaseViewModel)obj.GetValue(TabItemViewModelProperty);
        }

        public static void SetTabItemViewModel(DependencyObject obj, BaseViewModel value)
        {
            obj.SetValue(TabItemViewModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for TabItemViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabItemViewModelProperty =
            DependencyProperty.RegisterAttached("TabItemViewModel", typeof(BaseViewModel), typeof(AttachedProperties), new PropertyMetadata(null));


        #endregion

    }
}
