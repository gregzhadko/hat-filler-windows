﻿using Model.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Model
{
    public class PackService : IPackService
    {
        public void AddPhrase(int packId, PhraseItem phrase)
        {
            GetResponse(
                $"addPackWordDescription?id={packId}&word={phrase.Phrase}&description={phrase.Description.ReplaceSemicolons()}&level={phrase.Complexity}&author={phrase.ReviewedBy}", 8091);
        }

        public void DeletePhrase(int packId, string phrase, string author) => GetResponse($"removePackWord?id={packId}&word={phrase}&author={author}", 8091);

        public void EditPack(int id, string name, string description)
        {
            if (id == 0)
            {
                return;
            }

            GetResponse($"updatePackInfo?id={id}&name={name}&description={description.ReplaceSemicolons()}", 8091);
        }

        public void EditPhrase(int packId, PhraseItem oldPhrase, PhraseItem newPhrase, string selectedAuthor)
        {
            if (oldPhrase.Phrase != newPhrase.Phrase)
            {
                DeletePhrase(packId, oldPhrase.Phrase, selectedAuthor);
            }

            if (!string.Equals(oldPhrase.Phrase, newPhrase.Phrase, StringComparison.Ordinal) ||
                Math.Abs(oldPhrase.Complexity - newPhrase.Complexity) > 0.01 ||
                !string.Equals(oldPhrase.Description, newPhrase.Description, StringComparison.Ordinal))
            {
                GetResponse(
                    $"addPackWordDescription?id={packId}&word={newPhrase.Phrase}&description={newPhrase.Description.ReplaceSemicolons()}&level={newPhrase.Complexity}&author={selectedAuthor}",
                    8091);
            }
        }

        public void ReviewPhrase(int packId, PhraseItem phrase, string reviewerName, State state)
        {
            GetResponse($"reviewPackWord?id={packId}&word={phrase.Phrase}&author={reviewerName}&state={(int)state}", 8091);
        }

        public IEnumerable<Pack> GetAllPacksInfo()
        {
            var packsInfo = GetPacksInfo(8081);
            return packsInfo.Select(p => new Pack { Id = Convert.ToInt32(p["id"]), Name = p["name"].ToString() });
        }

        public Task<IEnumerable<Pack>> GetAllPackInfoAsync(int port = 8081)
        {
            var task = new Task<IEnumerable<Pack>>(GetAllPacksInfo);
            task.Start();
            return task;
        }

        public Pack GetPackById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            var response = GetResponse($"getPack?id={id}", 8081);

            var pack = JsonConvert.DeserializeObject<Pack>(response);
            if (pack.Phrases == null)
            {
                pack.Phrases = new List<PhraseItem>();
            }
            pack.Phrases = pack.Phrases.OrderBy(p => p.Phrase).ToList();
            return pack;
        }

        public Task<Pack> GetPackByIdAsync(int id)
        {
            var task = new Task<Pack>(() => GetPackById(id));
            task.Start();
            return task;
        }

        public List<Tuple<int, string>> GetPorts() => new List<Tuple<int, string>>
        {
            new Tuple<int, string>(8081, "test")
        };

        private static IEnumerable<JToken> GetPacksInfo(int port)
        {
            var response = GetResponse("getPacks", port);
            var jObject = JObject.Parse(response)["packs"];
            var packs = jObject.Select(i => i["pack"]);
            return packs;
        }

        public int[] GetPackIds()
        {
            return GetPacksInfo(8081).Select(p => Convert.ToInt32(p["id"])).ToArray();
        }

        private static string GetResponse(string requestUriString, int port)
        {
            var request = WebRequest.Create($"http://{Settings.Default.ServerAddress}:{port}/" + requestUriString);

            using (var response = (HttpWebResponse) request.GetResponse())
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

        public List<PhraseEditInfo> GetPackEditingInfo(Dictionary<int, string> packDictionary)
        {
            var response = GetResponse("packEditingInfo", 8091);
            var jArray = JArray.Parse(response);

            var result = new List<PhraseEditInfo>();
            foreach(var obj in jArray)
            {
                var info = obj.ToObject<PhraseEditInfo>();
                info.PackName = packDictionary[info.Pack];
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                info.Date = dtDateTime.AddSeconds(info.Timestamp).ToLocalTime();
                result.Add(info);
            }

            return result;
        }
    }
}