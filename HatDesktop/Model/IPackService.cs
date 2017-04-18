using System;
using System.Collections.Generic;

namespace HatDesktop.Model
{
    public interface IPackService
    {
        Pack GetPackById(int port, int id, out string error);
        void EditPack(int id, string name, string description, out string error);
        IEnumerable<Pack> GetAllPacksInfo(int port, out string error);
        void AddPhrase(int packId, PhraseItem phrase, out string error);
        void DeletePhrase(int packId, string phrase, out string error);
        List<Tuple<int, string>> GetPorts();
        void EditPhrase(int packId, PhraseItem oldPhrase, PhraseItem newPhrase, string selectedAuthor, out string error);
        void ReviewPhrase(int packId, PhraseItem phrase, string reviewerName, State state, out string error);
    }
}