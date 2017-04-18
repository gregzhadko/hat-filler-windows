using System.Windows;
using System.Windows.Controls;
using HatDesktop.Model;
using HatDesktop.ViewModels;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace HatDesktop.Views
{
    public class ReviewColumn : Telerik.Windows.Controls.GridViewColumn
    {
        public override FrameworkElement CreateCellElement(GridViewCell cell, object dataItem)
        {
            var viewModel = (MainViewModel)cell.ParentOfType<RadGridView>().DataContext;
            var author = viewModel.SelectedAuthor;
            var phrase = (PhraseItem) dataItem;
            var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
            stackPanel.Children.Add(CreateDeleteButton(phrase, author));
            stackPanel.Children.Add(CreateEditButton(phrase, author));
            stackPanel.Children.Add(CreateReviewButton(phrase, author));

            phrase.RaiseUpdateAuthor += Refresh;
            
            return stackPanel;
        }

        private static RadButton CreateReviewButton(PhraseItem phrase, string author)
        {
            var button = new RadButton
            {
                Content = "Accept",
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(3, 1, 3, 2),
                Visibility = phrase.IsReviewedBy(author) ? Visibility.Hidden : Visibility.Visible
            };
            button.Click += (o, args) => Button_Click(o, State.Accept);
            return button;
        }

        private static RadButton CreateEditButton(PhraseItem phrase, string author)
        {
            var button = new RadButton
            {
                Content = "To edit",
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(3, 1, 3, 2),
                Visibility = phrase.IsWantToEditBy(author) ? Visibility.Hidden : Visibility.Visible
            };
            button.Click += (o, args) => Button_Click(o, State.Edit);
            return button;
        }

        private static RadButton CreateDeleteButton(PhraseItem phrase, string author)
        {
            var button = new RadButton
            {
                Content = "To delete",
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(3, 1, 3, 2),
                Visibility = phrase.IsWantToDeleteBy(author) ? Visibility.Hidden : Visibility.Visible
            };
            button.Click += (o, args) => Button_Click(o, State.Delete);
            return button;
        }

        private static void Button_Click(object sender, State state)
        {
            var btn = (Button) sender;
            var grid = btn.ParentOfType<RadGridView>();
            var phrase = (PhraseItem) btn.DataContext;
            var viewModel = (MainViewModel) grid.DataContext;
            if (!viewModel.ReviewPhrase(phrase, state))
            {
                MessageBox.Show("Error on the phrase reviewing");
            }
            else
            {
                btn.IsEnabled = false;
            }
        }
    }
}
