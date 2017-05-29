using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace Model
{
    public class Pack
    {
        public int Id { get; set; }
        public string Language { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IList<PhraseItem> Phrases { get; set; } = new List<PhraseItem>();

        public override string ToString() => Name;

        public string WholeName => $"{Id}. {Name}";

    }
}