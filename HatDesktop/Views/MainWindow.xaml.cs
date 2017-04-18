using HatDesktop.ViewModels;

namespace HatDesktop.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        ///     Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

            // assign new KeyboardCommandProvider
            PhraseGrid.KeyboardCommandProvider = new CustomKeyboardCommandProvider(PhraseGrid);
        }
    }
}