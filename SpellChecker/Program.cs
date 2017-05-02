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
        private static List<string> _skippedPhrases = File.ReadAllLines("SkipDictionary.txt").ToList();

        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            _service = new PackService();
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Загружаю паки...");
            LoadPacks();
            Console.WriteLine("Загрузка паков завершена");

            var yandexSpellCheck = new YandexSpeller();

            using (var hunSpell = new Hunspell("ru_RU_ie.aff", "ru_RU_ie.dic"))
            {
                InitCustomDictionary(hunSpell);

                foreach (var pack in _packs)
                {
                    Console.WriteLine($"Смотрю пак {pack.Name}");
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
            var customWords = File.ReadAllLines("CustomDictionary.txt");
            foreach (var line in customWords)
            {
                hunSpell.Add(line);
            }
        }

        static void SpellPhrase(Pack pack, string phrase, Hunspell hunSpell, IYandexSpeller speller)
        {
            var words = GetWords(phrase);
            foreach (var word in words.Select(w => w.ToLowerInvariant()))
            {
                if (!hunSpell.Spell(word) && !ExistsInSkipped(word, phrase, pack.Id) && speller.CheckText(word, Lang.Ru | Lang.En, Options.IgnoreCapitalization, TextFormat.Plain).Errors.Any())
                {
                    var color = Console.ForegroundColor;
                    Console.Write($"{DateTime.Now:hh:mm:ss}: Ошибка в слове ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(word);
                    Console.ForegroundColor = color;
                    Console.Write(" из пака ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(pack.Name);
                    Console.ForegroundColor = color;
                    Console.Write(" Полная фраза: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(phrase);
                    Console.ForegroundColor = color;
                    Console.WriteLine($" Добавим слово в словарь или пропустим в конкретном случае? (d - dictionary, s - skip)");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(word);
                    Console.WriteLine();
                    Console.ForegroundColor = color;
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'y' || key.KeyChar == 'Y' || key.KeyChar == 'd' || key.KeyChar == 'D')
                    {
                        SaveNewCustomWord(hunSpell, word);
                    }
                    else if (key.KeyChar == 's' || key.KeyChar == 'S')
                    {
                        SaveNewSkipWord(word, phrase, pack.Id);
                    }
                    Console.WriteLine("Работаем Дальше!");
                }
            }
        }

        private static bool ExistsInSkipped(string word, string wholeWord, int id)
        {
            foreach(var line in _skippedPhrases.Select(s => s.Split(new[] { '|' })).ToArray())
            {
                if(String.Compare(line[0], word, true) == 0 && String.Compare(line[1], id.ToString(), true) == 0 && String.Compare(line[2], wholeWord, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SaveNewCustomWord(Hunspell hunSpell, string word)
        {
            hunSpell.Add(word);
            File.AppendAllLines(@"..\..\CustomDictionary.txt", new string[] { word });
            File.AppendAllLines(@"CustomDictionary.txt", new string[] { word });
            Console.WriteLine($"\nСлово {word} было добавлено в персональный словарь");
        }

        private static void SaveNewSkipWord(string word, string wholeWord, int packId)
        {
            var stringToSave = $"{word}|{packId}|{wholeWord}";
            File.AppendAllLines(@"..\..\SkipDictionary.txt", new string[] { stringToSave });
            File.AppendAllLines(@"SkipDictionary.txt", new string[] { stringToSave });
            Console.WriteLine($"\nСлово {word} было добавлено в словарь пропущенных слов");
        }

        static void LoadPacks()
        {
            var packs = _service.GetAllPacksInfo(8081, out string error);
            foreach(var pack in packs)
            {
                Console.WriteLine($"Загрузка пака {pack.Name}");
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
