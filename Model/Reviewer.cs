using GalaSoft.MvvmLight;
using System.Linq;
using System.Runtime.Serialization;

namespace Model
{
    [DataContract]
    public class Reviewer : ObservableObject
    {
        public Reviewer(string author, State reviewState = State.Unknown)
        {
            Author = author;
            ReviewState = reviewState;
        }

        [DataMember]
        public string Author { get; set; }

        public State ReviewState
        {
            get => _reviewState;
            set => Set(ref _reviewState, value);
        }

        public static readonly string[] DefaultReviewers = { "fomin", "tatarintsev", "sivykh", "zhadko" };
        private State _reviewState;

        public static Reviewer[] NewReviewers() => DefaultReviewers.Select(r => new Reviewer(r)).ToArray();
    }
}