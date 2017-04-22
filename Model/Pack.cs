using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace Model
{
    public class Pack : ObservableObject
    {
        public int Id { get; set; }
        public string Language { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IList<PhraseItem> Phrases { get; set; } = new List<PhraseItem>();

        public override string ToString() => Name;
    }
}