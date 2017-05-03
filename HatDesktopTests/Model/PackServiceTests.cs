using Faker;

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace HatDesktopTests.Model
{
    [TestFixture]
    public class PackServiceTests
    {
        private readonly IPackService _packService = new PackService();
        private static string _testAuthor = "zhadko";
        private const int Port = 8081;
        private const int Id = 15;

        [Test]
        public void GetPackInfoTest()
        {
            var pack = _packService.GetPackById(Id);
            Assert.IsNotNull(pack);
            Assert.IsNotEmpty(pack.Name);
            Assert.IsNotEmpty(pack.Description);
            Assert.IsNotNull(pack.Phrases);
        }

        [Test]
        public void EditPackTest()
        {
            var pack = _packService.GetPackById(Id);
            var newName = Name.First();
            var newDescription = Lorem.Paragraph(1);
            _packService.EditPack(Id, newName, newDescription);

            var newPack = _packService.GetPackById(Id);

            Assert.That(newPack.Name, Is.EqualTo(newName));
            Assert.That(newPack.Description, Is.EqualTo(newDescription));
            CollectionAssert.AreEqual(pack.Phrases.Select(p => p.Phrase), newPack.Phrases.Select(p => p.Phrase));

            _packService.EditPack(Id, pack.Name, pack.Description);
        }

        [Test]
        public void GetPacksIdsTest()
        {
            IList<Pack> packs = _packService.GetAllPacksInfo(Port).ToList();
            CollectionAssert.Contains(packs.Select(p => p.Id), 1);
            CollectionAssert.Contains(packs.Select(p => p.Id), 10);
            CollectionAssert.Contains(packs.Select(p => p.Id), 12);
        }

        [Test]
        public void AddSimplePhraseTest()
        {
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase);

            var newPack = _packService.GetPackById(Id);
            Assert.That(newPack.Phrases.Any(p => p.Phrase == phrase.Phrase));

            _packService.DeletePhrase(Id, phrase.Phrase);
        }

        [Test]
        public void DeleteSimplePhraseTest()
        {
            var pack = _packService.GetPackById(Id);
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase);
            _packService.DeletePhrase(Id, phrase.Phrase);
            var newPack = _packService.GetPackById(Id);
            Assert.That(pack.Phrases.Count, Is.EqualTo(newPack.Phrases.Count));
        }

        [Test]
        public void AddPhraseTest()
        {
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase);

            var newPack = _packService.GetPackById(Id);
            Assert.That(newPack.Phrases.Any(p => p.Phrase == phrase.Phrase && p.Description == phrase.Description));

            _packService.DeletePhrase(Id, phrase.Phrase);
        }

        [Test]
        public void DeletePhraseTest()
        {
            var pack = _packService.GetPackById(Id);
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase);
            _packService.DeletePhrase(Id, phrase.Phrase);
            var newPack = _packService.GetPackById(Id);
            Assert.That(pack.Phrases.Count, Is.EqualTo(newPack.Phrases.Count));
        }

        [Test]
        public void EditPhraseDescriptionTest()
        {
            var oldPhrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, oldPhrase);
            var newPhrase = new PhraseItem
            {
                Phrase = oldPhrase.Phrase,
                Description = $"{oldPhrase.Description} {Lorem.Sentence()}",
                Complexity = oldPhrase.Complexity % 5,
            };
            newPhrase.UpdateAuthor(_testAuthor);
            _packService.EditPhrase(Id, oldPhrase, newPhrase, _testAuthor);

            var pack = _packService.GetPackById(Id);
            Assert.That(pack.Phrases.Any(p => p.Phrase == newPhrase.Phrase && p.Description == newPhrase.Description));
            Assert.That(pack.Phrases.Select(p => p.Description), Is.Not.Contains(oldPhrase.Description));

            _packService.DeletePhrase(Id, newPhrase.Phrase);
        }

        [Test]
        public void EditPhraseTest()
        {
            var oldPhrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, oldPhrase);
            var newPhrase = new PhraseItem
            {
                Phrase = oldPhrase.Phrase + " " + Name.First(),
                Description = oldPhrase.Description + " " + Lorem.Sentence(),
                Complexity = oldPhrase.Complexity % 5,
            };
            newPhrase.UpdateAuthor(_testAuthor);
            _packService.EditPhrase(Id, oldPhrase, newPhrase, _testAuthor);

            var pack = _packService.GetPackById(Id);
            Assert.That(pack.Phrases.Any(p => p.Phrase == newPhrase.Phrase && p.Description == newPhrase.Description));
            Assert.That(pack.Phrases.Select(p => p.Phrase), Is.Not.Contains(oldPhrase.Phrase));
            Assert.That(pack.Phrases.Select(p => p.Description), Is.Not.Contains(oldPhrase.Description));

            _packService.DeletePhrase(Id, newPhrase.Phrase);
        }

        private static PhraseItem GenerateNewPhrase()
        {
            var phrase = new PhraseItem
            {
                Complexity = RandomNumber.Next(1, 5),
                Description = Lorem.Sentence(),
                Phrase = Name.First(),
            };
            phrase.UpdateAuthor(_testAuthor);
            return phrase;
        }

        [Test]
        public void GetPortsTest()
        {
            var ports = _packService.GetPorts();
            CollectionAssert.Contains(ports.Select(t => t.Item1), 8081);
        }

       [Test]
        public void PhraseCountTest()
        {
            //Get pack food
            var pack = _packService.GetPackById(9);
            Assert.That(pack.Phrases.Count, Is.GreaterThan(50));
        }
    }
}