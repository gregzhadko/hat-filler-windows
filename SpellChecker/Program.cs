using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Model;
using NHunspell;
using Yandex.Speller.Api;
using Yandex.Speller.Api.DataContract;
using System.IO;

namespace SpellChecker
{
    public class Program
    {
        private static PackService _service;
        private static List<Pack> _packs = new List<Pack>();

        public static void Main(string[] args)
        {

            _service = new PackService();
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Data loading...");
            LoadPacks();
            Console.WriteLine("Data loading is completed");

            var yandexSpellCheck = new YandexSpeller();

            using (var hunSpell = new Hunspell("ru_RU_ie.aff", "ru_RU_ie.dic"))
            {
                InitCustomDictionary(hunSpell);

                foreach (var pack in _packs)
                {
                    foreach (var phrase in pack.Phrases)
                    {
                        SpellPhrase(pack, phrase.Phrase, hunSpell, yandexSpellCheck);
                        SpellPhrase(pack, phrase.Description, hunSpell, yandexSpellCheck);
                    }
                }
            }
            Console.WriteLine("Spell check is finished");
            Console.ReadKey();
        }

        private static void InitCustomDictionary(Hunspell hunSpell)
        {
            string[] lines = File.ReadAllLines("CustomDictionary.txt");
            foreach (var line in lines)
            {
                hunSpell.Add(line);
            }
        }

        static void SpellPhrase(Pack pack, string phrase, Hunspell hunSpell, IYandexSpeller speller)
        {
            var words = GetWords(phrase);
            foreach (var word in words)
            {
                if (!hunSpell.Spell(word) && speller.CheckText(word, Lang.Ru | Lang.En, Options.IgnoreCapitalization, TextFormat.Plain).Errors.Any())
                {
                    Console.WriteLine($"{DateTime.Now:hh:mm:ss}: Ошибка в слове {word} из пака {pack.Name}. Полная фраза: {phrase}");
                    Console.WriteLine($"Может добавим в словарь слово {word}? y/n");
                    var key = Console.ReadKey();
                    Console.WriteLine();
                    if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                    {
                        SaveNewCustomWord(hunSpell, word);
                    }
                    Console.WriteLine("Работаем Дальше!");
                }
            }
        }

        private static void SaveNewCustomWord(Hunspell hunSpell, string word)
        {
            hunSpell.Add(word);
            File.AppendAllLines(@"..\..\CustomDictionary.txt", new string[] { word });
            Console.WriteLine($"Слово {word} было добавлено");
        }

        static void LoadPacks()
        {
            var packs = _service.GetAllPacksInfo(8081, out string error);
            foreach(var pack in packs)
            {
                Console.WriteLine($"Loading of pack {pack.Name}");
                _packs.Add(_service.GetPackById(8081, pack.Id, out error));
            }
        }

        static string[] GetWords(string input)
        {
            var matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}
