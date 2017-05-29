using System;
using System.Collections.Generic;

namespace Model
{
    public interface IPackService
    {
        Pack GetPackById(int id);

        void EditPack(int id, string name, string description);

        IEnumerable<Pack> GetAllPacksInfo();

        void AddPhrase(int packId, PhraseItem phrase);

        void DeletePhrase(int packId, string phrase);

        List<Tuple<int, string>> GetPorts();

        void EditPhrase(int packId, PhraseItem oldPhrase, PhraseItem newPhrase, string selectedAuthor);

        void ReviewPhrase(int packId, PhraseItem phrase, string reviewerName, State state);
    }
}