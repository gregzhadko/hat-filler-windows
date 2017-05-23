using System.Windows;
using System.Windows.Controls;
using HatNewUI.Helpers;
using HatNewUI.ViewModel;

namespace HatNewUI
{
    /// <summary>
    /// Interaction logic for GridBasedView.xaml
    /// </summary>
    public partial class GridBasedView : UserControl
    {
        public
            GridBasedView()
        {
            InitializeComponent();
        }

        #region GridContent DP
        public object GridContent
        {
            get => (object)GetValue(GridContentProperty);
            set => SetValue(GridContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for GridContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridContentProperty =
            DependencyProperty.Register("GridContent", typeof(object), typeof(GridBasedView), new PropertyMetadata(null, ContentPropertyBeingSet));

        private static void ContentPropertyBeingSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depObj = e.NewValue as DependencyObject;
            if (depObj == null) return;
            AttachedProperties.SetVisualParent(depObj, d);
        }
        #endregion

        #region GridBasedDataContext DP
        public GridBasedViewModel GridBasedDataContext
        {
            get => (GridBasedViewModel)GetValue(GridBasedDataContextProperty);
            set => SetValue(GridBasedDataContextProperty, value);
        }

        // Using a DependencyProperty as the backing store for GridBasedDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridBasedDataContextProperty =
            DependencyProperty.Register("GridBasedDataContext", typeof(GridBasedViewModel), typeof(GridBasedView), new PropertyMetadata(null));

        #endregion


    }
}
