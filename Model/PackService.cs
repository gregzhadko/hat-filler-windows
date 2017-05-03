using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Model.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class PackService : IPackService
    {
        public void AddPhrase(int packId, PhraseItem phrase)
        {
            GetResponceFromServer(
                $"addPackWordDescription?id={packId}&word={phrase.Phrase}&description={phrase.Description}&level={phrase.Complexity}&author={phrase.ReviewedBy}", 8091);
        }

        public void DeletePhrase(int packId, string phrase) => GetResponceFromServer($"removePackWord?id={packId}&word={phrase}", 8091);

        public void EditPack(int id, string name, string description)
        {
            if (id == 0)
            {
                return;
            }

            GetResponceFromServer($"updatePackInfo?id={id}&name={name}&description={description}", 8091);
        }

        public void EditPhrase(int packId, PhraseItem oldPhrase, PhraseItem newPhrase, string selectedAuthor)
        {
            if (oldPhrase.Phrase != newPhrase.Phrase)
            {
                DeletePhrase(packId, oldPhrase.Phrase);
            }

            if (!string.Equals(oldPhrase.Phrase, newPhrase.Phrase, StringComparison.Ordinal) ||
                Math.Abs(oldPhrase.Complexity - newPhrase.Complexity) > 0.01 ||
                !string.Equals(oldPhrase.Description, newPhrase.Description, StringComparison.Ordinal))
            {
                GetResponceFromServer(
                    $"addPackWordDescription?id={packId}&word={newPhrase.Phrase}&description={newPhrase.Description}&level={newPhrase.Complexity}&author={selectedAuthor}",
                    8091);
            }
        }

        public void ReviewPhrase(int packId, PhraseItem phrase, string reviewerName, State state)
        {
            GetResponceFromServer($"reviewPackWord?id={packId}&word={phrase.Phrase}&author={reviewerName}&state={(int)state}", 8091);
        }

        public IEnumerable<Pack> GetAllPacksInfo(int port)
        {
            var packsInfo = GetPacksInfo(port);
            return packsInfo.Select(p => new Pack { Id = Convert.ToInt32(p["id"]), Name = p["name"].ToString() });
        }

        public Pack GetPackById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            var response = GetResponceFromServer($"getPack?id={id}", 8081);

            var pack = JsonConvert.DeserializeObject<Pack>(response);
            if (pack.Phrases == null)
            {
                pack.Phrases = new List<PhraseItem>();
            }
            pack.Phrases = pack.Phrases.OrderBy(p => p.Phrase).ToList();
            return pack;
        }

        public List<Tuple<int, string>> GetPorts() => new List<Tuple<int, string>>
        {
            new Tuple<int, string>(8081, "test")
        };

        private static IEnumerable<JToken> GetPacksInfo(int port)
        {
            var response = GetResponceFromServer("getPacks", port);
            var jObject = JObject.Parse(response)["packs"];
            var packs = jObject.Select(i => i["pack"]);
            return packs;
        }

        public int[] GetPackIds()
        {
            return GetPacksInfo(8081).Select(p => Convert.ToInt32(p["id"])).ToArray();
        }

        private static string GetResponceFromServer(string requestUriString, int port)
        {
            var request = WebRequest.Create($"http://{Settings.Default.ServerAddress}:{port}/" + requestUriString);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var dataStream = response.GetResponseStream())
            {
                if (dataStream == null || dataStream == Stream.Null)
                {
                    throw new WebException("Stream is null");
                }

                var reader = new StreamReader(dataStream);
                return reader.ReadToEnd();
            }
            
        }

    }
}