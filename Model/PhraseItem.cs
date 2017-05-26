using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Model
{
    public class PhraseItem : IDataErrorInfo, ICloneable
    {
        public Dictionary<string, int> Reviews
        {
            set
            {
                if (value == null)
                {
                    return;
                }

                foreach (var authorState in value)
                {
                    var reviewer = ReviewerObjects.FirstOrDefault(r => r.Author == authorState.Key);
                    if (reviewer != null)
                    {
                        reviewer.ReviewState = (State) authorState.Value;
                    }
                }
            }
        }

        public Reviewer[] ReviewerObjects { get; set; } = Reviewer.NewReviewers();

        public double Complexity { get; set; }

        public string Description { get; set; }

        public string Phrase { get; set; }

        public bool IsNew { get; set; } = false;

        public bool IsValid => string.IsNullOrEmpty(Error);

        public string ReviewedBy => ReviewerObjects.FirstOrDefault(r => r.ReviewState == State.Accept)?.Author;

        public string Error => this[null];

        public string this[string columnName]
        {
            get
            {
                var result = new StringBuilder();

                if (columnName == null || columnName == nameof(Phrase))
                {
                    if (string.IsNullOrEmpty(Phrase))
                    {
                        result.AppendLine("Please enter a phrase");
                    }
                    else if(!Phrase.All(c => Char.IsLetterOrDigit(c) || c == ' '))
                    {
                        result.AppendLine("Phrase should contain only Russian or Latin letters or numbers");
                    }
                }

                if (columnName == null || columnName == nameof(Complexity))
                {
                    if (Complexity > 5 || Complexity < 1)
                    {
                        result.AppendLine("Complexity should be in range [1, 5]");
                    }
                }

                if (columnName == null || columnName == nameof(Description))
                {
                    if (string.IsNullOrEmpty(Description))
                    {
                        result.AppendLine("Please enter a Description");
                    }
                }

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

        public object Clone()
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

        public bool IsReviewedBy(string author)
        {
            return ReviewerObjects.First(r => r.Author == author).ReviewState == State.Accept;
        }

        public bool IsWantToEditBy(string author)
        {
            return ReviewerObjects.First(r => r.Author == author).ReviewState == State.Edit;
        }

        public bool IsWantToDeleteBy(string author)
        {
            return ReviewerObjects.First(r => r.Author == author).ReviewState == State.Delete;
        }

    }
}