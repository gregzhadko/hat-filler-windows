using System.Windows;

namespace HatDesktop.Views
{
    /// <summary>
    ///     Description for RenamePackView.
    /// </summary>
    public partial class RenamePackView : IModalWindow
    {
        /// <summary>
        ///     Initializes a new instance of the RenamePackView class.
        /// </summary>
        public RenamePackView()
        {
            InitializeComponent();
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}