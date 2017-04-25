using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Model;
using NHunspell;
using Yandex.Speller.Api;
using Yandex.Speller.Api.DataContract;

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

            IYandexSpeller speller = new YandexSpeller();

            using (var hunspell = new Hunspell("ru_RU_ie.aff", "ru_RU_ie.dic"))
            {
                foreach (var pack in _packs)
                {
                    foreach (var phrase in pack.Phrases)
                    {
                        var words = GetWords(phrase.Phrase);
                        foreach (var word in words)
                        {
                            if (!hunspell.Spell(word) && speller.CheckText(word, Lang.Ru | Lang.En, Options.IgnoreCapitalization, TextFormat.Plain).Errors.Any())
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss}: Ошибка в слове {word} из пака {pack.Name}. Полное слово: {phrase.Phrase}");
                                //Console.WriteLine($"Может добавим в словарь слово {word}? y/n");
                                //var key = Console.ReadKey();
                                //if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                                //{
                                //    hunspell.Add(word);
                                //}
                            }
                        }

                        words = GetWords(phrase.Description);
                        foreach (var word in words)
                        {
                            if (!hunspell.Spell(word) && speller.CheckText(word, Lang.Ru | Lang.En, Options.IgnoreCapitalization, TextFormat.Plain).Errors.Any())
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss}: Ошибка в слове {word} из пака {pack.Name}. Из описания к слову: {phrase.Phrase}");
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Spellcheck is finished");
            Console.ReadKey();
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
