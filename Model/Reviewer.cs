using System.Linq;
using GalaSoft.MvvmLight;

namespace Model
{
    public class Reviewer : ObservableObject
    {
        public Reviewer(string author, State reviewState = State.Unknown)
        {
            Author = author;
            ReviewState = reviewState;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Author { get; set; }

        public State ReviewState
        {
            get { return _reviewState; }
            set { Set(ref _reviewState, value); }
        }

        public static readonly string[] DefaultReviewers = { "fomin", "tatarintsev", "sivykh", "zhadko" };
        private State _reviewState;

        public static Reviewer[] NewReviewers() => DefaultReviewers.Select(r => new Reviewer(r)).ToArray();
    }
}