using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Model.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class PackService : IPackService
    {
        public void AddPhrase(int packId, PhraseItem phrase, out string error)
        {
            GetResponceFromServer($"addPackWordDescription?id={packId}&word={phrase.Phrase}&description={phrase.Description}&level={phrase.Complexity}&author={phrase.ReviewedBy}", 8091, out error);
        }

        public void DeletePhrase(int packId, string phrase, out string error) => GetResponceFromServer($"removePackWord?id={packId}&word={phrase}", 8091, out error);

        public void EditPack(int id, string name, string description, out string error)
        {
            error = "";
            if (id == 0)
            {
                return;
            }

            GetResponceFromServer($"updatePackInfo?id={id}&name={name}&description={description}", 8091, out error);
        }

        public void EditPhrase(int packId, PhraseItem oldPhrase, PhraseItem newPhrase, string selectedAuthor,
            out string error)
        {
            error = "";
            if (oldPhrase.Phrase != newPhrase.Phrase)
            {
                DeletePhrase(packId, oldPhrase.Phrase, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    return;
                }
            }

            if (!string.Equals(oldPhrase.Phrase, newPhrase.Phrase, StringComparison.Ordinal) ||
                Math.Abs(oldPhrase.Complexity - newPhrase.Complexity) > 0.01 ||
                !string.Equals(oldPhrase.Description, newPhrase.Description, StringComparison.Ordinal))
            {
                GetResponceFromServer(
                    $"addPackWordDescription?id={packId}&word={newPhrase.Phrase}&description={newPhrase.Description}&level={newPhrase.Complexity}&author={selectedAuthor}",
                    8091, out error);
            }
        }

        public void ReviewPhrase(int packId, PhraseItem phrase, string reviewerName, State state, out string error)
        {
            GetResponceFromServer($"reviewPackWord?id={packId}&word={phrase.Phrase}&author={reviewerName}&state={(int)state}", 8091, out error);
        }

        public IEnumerable<Pack> GetAllPacksInfo(int port, out string error)
        {
            var packsInfo = GetPacksInfo(port, out error);
            return packsInfo.Select(p => new Pack { Id = Convert.ToInt32(p["id"]), Name = p["name"].ToString() });
        }

        public Pack GetPackById(int port, int id, out string error)
        {
            error = "";
            if (id == 0)
            {
                return null;
            }

            var response = GetResponceFromServer($"getPack?id={id}", port, out error);

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

        public Task<Pack[]> GetFullPacksData(int port)
        {
            var packIds = GetPacksInfo(port, out string error).Select(p => Convert.ToInt32(p["id"])).ToList();
            var loadTasks = new List<Task<Pack>>();
            for (int i = 0; i < packIds.Count; i++)
            {
                var id = i;
                var task = new Task<Pack>(() =>
                {
                    return GetPackById(port, packIds[id], out string e);
                });
                loadTasks.Add(task);
            }

            return Task.WhenAll(loadTasks);
        }

        private static IEnumerable<JToken> GetPacksInfo(int port, out string error)
        {
            var response = GetResponceFromServer("getPacks", port, out error);
            var jObject = JObject.Parse(response)["packs"];
            var packs = jObject.Select(i => i["pack"]);
            return packs;
        }

        private static string GetResponceFromServer(string requestUriString, int port, out string errorResponse)
        {
            string responseFromServer = null;
            HttpWebResponse response = null;
            errorResponse = null;

            var request = WebRequest.Create($"http://{Settings.Default.ServerAddress}:{port}/" + requestUriString);

            try
            {
                response = (HttpWebResponse)request.GetResponse();

                using (var dataStream = response.GetResponseStream())
                {
                    if (dataStream == null || dataStream == Stream.Null)
                    {
                        throw new WebException("Stream is null");
                    }

                    var reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                errorResponse = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
            finally
            {
                response?.Dispose();
            }

            return responseFromServer;
        }
    }
}