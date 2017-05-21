using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Model
{
    public class PhraseItem : ObservableObject, IDataErrorInfo
    {
        private double _complexity;
        private string _description;

        private string _phrase;

        // ReSharper disable once UnusedMember.Global
        public Dictionary<string, int> Reviews
        {
            //get { return ReviewerObjects.Select(r => r.Author).ToArray(); }
            set
            {
                if (value == null)
                    return;
                foreach (var authorState in value)
                {
                    var reviewer = ReviewerObjects.FirstOrDefault(r => r.Author == authorState.Key);
                    if (reviewer != null)
                    {
                        reviewer.ReviewState = (State)authorState.Value;
                    }
                }
            }
        }

        public Reviewer[] ReviewerObjects { get; set; } = Reviewer.NewReviewers();

        public double Complexity
        {
            get => _complexity;
            set => Set(nameof(Complexity), ref _complexity, value);
        }

        public string Description
        {
            get => _description;
            set => Set(nameof(Description), ref _description, value);
        }

        public string Phrase
        {
            get => _phrase;
            set => Set(nameof(Phrase), ref _phrase, value);
        }

        public bool IsNew { get; set; }

        public bool IsValid => string.IsNullOrEmpty(Error);

        public string ReviewedBy => ReviewerObjects.FirstOrDefault(r => r.ReviewState == State.Accept)?.Author;

        public string Error => this[null];

        public string this[string columnName]
        {
            get
            {
                var result = new StringBuilder();

                if (columnName == null || columnName == nameof(Phrase))
                    if (string.IsNullOrEmpty(Phrase))
                        result.AppendLine("Please enter a phrase");

                if (columnName == null || columnName == nameof(Complexity))
                    if (Complexity > 5 || Complexity < 1)
                        result.AppendLine("Complexity should be in [1, 5]");

                if (columnName == null || columnName == nameof(Description))
                    if (string.IsNullOrEmpty(Description))
                        result.AppendLine("Please enter a Description");

                return result.ToString();
            }
        }

        public void UpdateAuthor(string author)
        {
            foreach (var reviewer in ReviewerObjects)
            {
                reviewer.ReviewState = author == reviewer.Author ? State.Accept : State.Unknown;
            }

            RaiseUpdateAuthor?.Invoke();
        }

        public event Action RaiseUpdateAuthor;

        public PhraseItem Clone()
        {
            var phrase = new PhraseItem
            {
                Phrase = Phrase,
                Complexity = Complexity,
                Description = Description,
                ReviewerObjects = new Reviewer[ReviewerObjects.Length]
            };
            for (var i = 0; i < ReviewerObjects.Length; i++)
            {
                phrase.ReviewerObjects[i] = new Reviewer(ReviewerObjects[i].Author, ReviewerObjects[i].ReviewState);
            }

            return phrase;
        }

        public bool IsReviewedBy(string author) => ReviewerObjects.First(r => r.Author == author).ReviewState == State.Accept;

        public bool IsWantToEditBy(string author) => ReviewerObjects.First(r => r.Author == author).ReviewState == State.Edit;

        public bool IsWantToDeleteBy(string author) => ReviewerObjects.First(r => r.Author == author).ReviewState == State.Delete;
    }
}