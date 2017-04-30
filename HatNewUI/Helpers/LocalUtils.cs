using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using MVVMBase;

//using System.Windows.Media;

namespace HatNewUI.Helpers
{
    public static class LocalUtils
    {
        public static async void ShowProgressing(this BaseViewModel vm,
            Action<ProgressDialogController> act, string title = "Loading", string message = "Please wait...")
        {
            await ((App) Application.Current).RealMainWindow
                .ShowProgressAsync(title, message)
                .ContinueWith(x =>
                {
                    var progressController = x.Result;
                    act(progressController);
                    return progressController;
                }).ContinueWith(x => x.Result.CloseAsync());
        }

        public static async void ShowMessage(this BaseViewModel vm,
             string message = "Please wait...", Action<MessageDialogResult> afterMessage = null,
            string title = "Clinical Champion", MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings extraSettings = null)
        {
            await ((App) Application.Current).RealMainWindow
                .ShowMessageAsync(title, message, style, extraSettings)
                .ContinueWith(x => afterMessage?.Invoke(x.Result));
        }

        public static int FindIndex<T>(this IList<T> source, Predicate<T> match, int startIndex = 0)
        {
            for (var i = startIndex; i < source.Count; i++)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }


        public static void SelectCellByIndex(this DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            object item = dataGrid.Items[rowIndex]; //=Product X
            DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row == null)
            {
                dataGrid.ScrollIntoView(item);
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }
            if (row != null)
            {
                DataGridCell cell = GetCell(dataGrid, row, columnIndex);
                if (cell != null)
                {
                    var content = (cell.Content as IInputElement);
                    if (content != null)
                    {
                        content.Focus();
                    }
                }
            }
        }

        public static DataGridCell GetCell(this DataGrid dataGrid, DataGridRow rowContainer, int column)
        {
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    /* if the row has been virtualized away, call its ApplyTemplate() method
                     * to build its visual tree in order for the DataGridCellsPresenter
                     * and the DataGridCells to be created */
                    rowContainer.ApplyTemplate();
                    presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                if (presenter != null)
                {
                    DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    if (cell == null)
                    {
                        /* bring the column into view
                         * in case it has been virtualized away */
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                        cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    }
                    return cell;
                }
            }
            return null;
        }
        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }



        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var childT = child as T;
                if (childT != null) yield return childT;

                foreach (var childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
            }
        }


        public static T Deserialize<T>(string oSource, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            using (Stream stream = new MemoryStream())
            {
                var data = encoding.GetBytes(oSource);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractSerializer(typeof(T));
                return (T)deserializer.ReadObject(stream);
            }
        }

        public static string SerializeToString<T>(this T oSource)
        {
            var dcs = new DataContractSerializer(typeof(T));

            using (var ms = new MemoryStream())
            {
                dcs.WriteObject(ms, oSource);
                ms.Position = 0;
                return new StreamReader(ms).ReadToEnd();
            }

        }

        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent != null)
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child.GetType() == typeof(T))
                        return child as T;
                    var result = FindVisualChild<T>(child);
                    if (result != null) return result;
                }
            return null;
        }

        public static T FindParentPanel<T>(this DependencyObject o) where T : DependencyObject
        {
            var parent = o == null ? null : VisualTreeHelper.GetParent(o);

            if (parent != null)
            {
                if (parent.GetType() == typeof(T))
                {
                    return parent as T;
                }

                return FindParentPanel<T>(parent);
            }

            return null;
        }

        public static void StartEditingRow(this DataGrid dataGrid, int itemIndex)
        {
            dataGrid.FocusRow(itemIndex);
            dataGrid.BeginEdit();
        }

        public static int GetCurrentColumnIndex(this DataGrid dataGrid)
        {
            return dataGrid == null || dataGrid.CurrentColumn == null ? 0 :
                dataGrid.CurrentColumn.DisplayIndex;
        }

        public static void FocusRow(this DataGrid dataGrid, int rowIndex)
        {
            var column = dataGrid.SelectedCells.Count == 0 ? 0 :
                    dataGrid.SelectedCells.First().Column.DisplayIndex;

            var cell = dataGrid.GetCell(rowIndex, column);
            if (cell == null) return;
            dataGrid.SelectedIndex = rowIndex;
            cell.Focus();
        }

        public static DataGridCell GetCell(this DataGrid grid, int row, int column)
        {
            var rowContainer = grid.GetRow(row);
            if (rowContainer == null) return null;

            var presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
            if (presenter == null) return null;

            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
            if (cell == null)
            {
                grid.ScrollIntoView(rowContainer, grid.Columns[column]);
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
            }
            return cell;
        }

        public static DataGridRow GetRow(this DataGrid grid, int index)
        {
            var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if (child != null) break;
            }
            return child;
        }

        public static DataGridCell GetLastColumnCell(this DataGrid dataGrid)
        {
            return dataGrid.GetCell(dataGrid.GetRow(dataGrid.SelectedIndex), dataGrid.Columns.Count - 1);
        }
    }
}
