using Faker;
using HatDesktop.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

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
            var pack = _packService.GetPackById(Port, Id, out string error);
            Assert.IsNotNull(pack);
            Assert.IsNotEmpty(pack.Name);
            Assert.IsNotEmpty(pack.Description);
            Assert.IsNotNull(pack.Phrases);
        }

        [Test]
        public void EditPackTest()
        {
            var pack = _packService.GetPackById(Port, Id, out string error);
            var newName = Name.First();
            var newDescription = Lorem.Paragraph(1);
            _packService.EditPack(Id, newName, newDescription, out error);

            var newPack = _packService.GetPackById(Port, Id, out error);

            Assert.That(newPack.Name, Is.EqualTo(newName));
            Assert.That(newPack.Description, Is.EqualTo(newDescription));
            CollectionAssert.AreEqual(pack.Phrases.Select(p => p.Phrase), newPack.Phrases.Select(p => p.Phrase));

            _packService.EditPack(Id, pack.Name, pack.Description, out error);
        }

        [Test]
        public void GetPacksIdsTest()
        {
            IList<Pack> packs = _packService.GetAllPacksInfo(Port, out string error).ToList();
            Assert.That(error, Is.Null.Or.Empty);
            CollectionAssert.Contains(packs.Select(p => p.Id), 1);
            CollectionAssert.Contains(packs.Select(p => p.Id), 10);
            CollectionAssert.Contains(packs.Select(p => p.Id), 12);
        }

        [Test]
        public void AddSimplePhraseTest()
        {
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase, out string error);

            var newPack = _packService.GetPackById(Port, Id, out error);
            Assert.That(newPack.Phrases.Any(p => p.Phrase == phrase.Phrase));

            _packService.DeletePhrase(Id, phrase.Phrase, out error);
        }

        [Test]
        public void DeleteSimplePhraseTest()
        {
            var pack = _packService.GetPackById(Port, Id, out string error);
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase, out error);
            _packService.DeletePhrase(Id, phrase.Phrase, out error);
            var newPack = _packService.GetPackById(Port, Id, out error);
            Assert.That(pack.Phrases.Count, Is.EqualTo(newPack.Phrases.Count));
        }

        [Test]
        public void AddPhraseTest()
        {
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase, out string error);

            var newPack = _packService.GetPackById(Port, Id, out error);
            Assert.That(newPack.Phrases.Any(p => p.Phrase == phrase.Phrase && p.Description == phrase.Description));

            _packService.DeletePhrase(Id, phrase.Phrase, out error);
        }

        [Test]
        public void DeletePhraseTest()
        {
            var pack = _packService.GetPackById(Port, Id, out string error);
            var phrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, phrase, out error);
            _packService.DeletePhrase(Id, phrase.Phrase, out error);
            var newPack = _packService.GetPackById(Port, Id, out error);
            Assert.That(pack.Phrases.Count, Is.EqualTo(newPack.Phrases.Count));
        }

        [Test]
        public void EditPhraseDescriptionTest()
        {
            var oldPhrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, oldPhrase, out string error);
            var newPhrase = new PhraseItem
            {
                Phrase = oldPhrase.Phrase,
                Description = $"{oldPhrase.Description} {Lorem.Sentence()}",
                Complexity = oldPhrase.Complexity % 5,
            };
            newPhrase.UpdateAuthor(_testAuthor);
            _packService.EditPhrase(Id, oldPhrase, newPhrase, _testAuthor, out error);

            var pack = _packService.GetPackById(Port, Id, out error);
            Assert.That(pack.Phrases.Any(p => p.Phrase == newPhrase.Phrase && p.Description == newPhrase.Description));
            Assert.That(pack.Phrases.Select(p => p.Description), Is.Not.Contains(oldPhrase.Description));

            _packService.DeletePhrase(Id, newPhrase.Phrase, out error);
        }

        [Test]
        public void EditPhraseTest()
        {
            var oldPhrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, oldPhrase, out string error);
            var newPhrase = new PhraseItem
            {
                Phrase = oldPhrase.Phrase + " " + Name.First(),
                Description = oldPhrase.Description + " " + Lorem.Sentence(),
                Complexity = oldPhrase.Complexity % 5,
            };
            newPhrase.UpdateAuthor(_testAuthor);
            _packService.EditPhrase(Id, oldPhrase, newPhrase, _testAuthor, out error);

            var pack = _packService.GetPackById(Port, Id, out error);
            Assert.That(pack.Phrases.Any(p => p.Phrase == newPhrase.Phrase && p.Description == newPhrase.Description));
            Assert.That(pack.Phrases.Select(p => p.Phrase), Is.Not.Contains(oldPhrase.Phrase));
            Assert.That(pack.Phrases.Select(p => p.Description), Is.Not.Contains(oldPhrase.Description));

            _packService.DeletePhrase(Id, newPhrase.Phrase, out error);
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
        public void EditPhraseNameTest()
        {
            var originalPhrase = GenerateNewPhrase();
            _packService.AddPhrase(Id, originalPhrase, out string error);
            var newPhrase = new PhraseItem() { Phrase = Name.First(), Description = originalPhrase.Description, Complexity = originalPhrase.Complexity };
            newPhrase.UpdateAuthor(_testAuthor);
            _packService.EditPhrase(Id, originalPhrase, newPhrase, _testAuthor, out error);

            Assert.That(error, Is.Null.Or.Empty);

            var phrases = _packService.GetPackById(Port, Id, out error).Phrases.Select(p => p.Phrase).ToList();
            CollectionAssert.DoesNotContain(phrases, originalPhrase.Phrase);
            CollectionAssert.Contains(phrases, newPhrase.Phrase);

            _packService.DeletePhrase(Id, newPhrase.Phrase, out error);
            Assert.That(error, Is.Null.Or.Empty);

            phrases = _packService.GetPackById(Port, Id, out error).Phrases.Select(p => p.Phrase).ToList();
            Assert.That(phrases, Is.Not.Contains(newPhrase.Phrase));
        }

        //[Test()]
        //public void EditPhraseDescriptionTest()
        //{
        //    string error;
        //    var originalPhrase = GenerateNewPhrase();
        //    _packService.AddPhrase(Id, originalPhrase, out error);
        //    var newPhrase = new PhraseItem() { Phrase = originalPhrase.Phrase, Description = Faker.Lorem.Paragraph(), Complexity = originalPhrase.Complexity };
        //    _packService.EditPhrase(Id, originalPhrase, newPhrase, out error);

        //    Assert.That(error, Is.Null.Or.Empty);

        //    var descriptions = _packService.GetPackById(Port, Id, out error).Phrases.Select(p => p.Description).ToList();
        //    CollectionAssert.DoesNotContain(descriptions, originalPhrase.Description);
        //    CollectionAssert.Contains(descriptions, newPhrase.Description);

        //    _packService.DeletePhrase(Id, newPhrase.Phrase, out error);
        //    Assert.That(error, Is.Null.Or.Empty);

        //    descriptions = _packService.GetPackById(Port, Id, out error).Phrases.Select(p => p.Description).ToList();
        //    Assert.That(descriptions, Is.Not.Contains(newPhrase.Description));
        //}

        //[Test()]
        //public void EditPhraseTest()
        //{
        //    string error;
        //    var originalPhrase = GenerateNewPhrase();
        //    _packService.AddPhrase(Id, originalPhrase, out error);
        //    var newPhrase = new PhraseItem() { Phrase = Faker.Name.First(), Description = Faker.Lorem.Paragraph(), Complexity = originalPhrase.Complexity };
        //    _packService.EditPhrase(Id, originalPhrase, newPhrase, out error);

        //    Assert.That(error, Is.Null.Or.Empty);

        //    var phrases = _packService.GetPackById(Port, Id, out error).Phrases;
        //    CollectionAssert.DoesNotContain(phrases.Select(p => p.Phrase), originalPhrase.Phrase);
        //    CollectionAssert.DoesNotContain(phrases.Select(p => p.Description), originalPhrase.Description);
        //    CollectionAssert.Contains(phrases.Select(p => p.Phrase), newPhrase.Phrase);
        //    CollectionAssert.Contains(phrases.Select(p => p.Description), newPhrase.Description);

        //    _packService.DeletePhrase(Id, newPhrase.Phrase, out error);
        //    Assert.That(error, Is.Null.Or.Empty);

        //    phrases = _packService.GetPackById(Port, Id, out error).Phrases;
        //    Assert.That(phrases.Select(p => p.Phrase), Is.Not.Contains(newPhrase.Phrase));
        //    Assert.That(phrases.Select(p => p.Description), Is.Not.Contains(newPhrase.Description));
        //}

        [Test]
        public void PhraseCountTest()
        {
            //Get pack food
            var pack = _packService.GetPackById(Port, 9, out string error);
            Assert.That(pack.Phrases.Count, Is.GreaterThan(50));
        }
    }
}