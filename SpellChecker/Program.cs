using Model;
using NHunspell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yandex.Speller.Api;
using Yandex.Speller.Api.DataContract;

namespace SpellChecker
{
    public class Program
    {
        private static PackService _service;
        private static readonly List<Pack> Packs = new List<Pack>();
        private static readonly List<string> SkippedPhrases = File.ReadAllLines("SkipDictionary.txt").ToList();

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

                foreach (var pack in Packs)
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

        private static void SpellPhrase(Pack pack, string phrase, Hunspell hunSpell, IYandexSpeller speller)
        {
            var words = GetWords(phrase);
            foreach (var word in words.Select(w => w.ToLowerInvariant()))
            {
                if (hunSpell.Spell(word) || ExistsInSkipped(word, phrase, pack.Id))
                {
                    continue;
                }
                if (YandexSpellCheck(speller, word))
                {
                    ShowSpellErrorMessages(pack, phrase, word);
                    var key = Console.ReadKey();
                    switch (key.KeyChar)
                    {
                        case 'y':
                        case 'Y':
                        case 'd':
                        case 'D':
                            SaveNewCustomWord(hunSpell, word);
                            break;
                        case 's':
                        case 'S':
                            SaveNewSkipWord(word, phrase, pack.Id);
                            break;
                    }
                    Console.WriteLine("Работаем Дальше!");
                }
                else
                {
                    SaveNewCustomWord(hunSpell, word);
                }
            }
        }

        private static bool YandexSpellCheck(IYandexSpeller speller, string word)
        {
            return speller.CheckText(word, Lang.Ru | Lang.En, Options.IgnoreCapitalization, TextFormat.Plain).Errors.Any();
        }

        private static void ShowSpellErrorMessages(Pack pack, string phrase, string word)
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
            Console.WriteLine(" Добавим слово в словарь или пропустим в конкретном случае? (d - dictionary, s - skip)");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(word);
            Console.WriteLine();
            Console.ForegroundColor = color;
        }

        private static bool ExistsInSkipped(string word, string wholeWord, int id)
        {
            return SkippedPhrases.Select(s => s.Split('|')).Any(line => string.Compare(line[0], word, StringComparison.OrdinalIgnoreCase) == 0 &&
                                                                         string.Compare(line[1], id.ToString(), StringComparison.OrdinalIgnoreCase) == 0 &&
                                                                         string.Compare(line[2], wholeWord, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static void SaveNewCustomWord(Hunspell hunSpell, string word)
        {
            hunSpell.Add(word);
            File.AppendAllLines(@"..\..\CustomDictionary.txt", new[] {word});
            File.AppendAllLines(@"CustomDictionary.txt", new[] {word});
            Console.WriteLine($"\nСлово {word} было добавлено в персональный словарь");
        }

        private static void SaveNewSkipWord(string word, string wholeWord, int packId)
        {
            var stringToSave = $"{word}|{packId}|{wholeWord}";
            File.AppendAllLines(@"..\..\SkipDictionary.txt", new[] {stringToSave});
            File.AppendAllLines(@"SkipDictionary.txt", new[] {stringToSave});
            Console.WriteLine($"\nСлово {word} было добавлено в словарь пропущенных слов");
        }

        private static void LoadPacks()
        {
            var packs = _service.GetAllPacksInfo(8081);
            foreach(var pack in packs)
            {
                Console.WriteLine($"Загрузка пака {pack.Name}");
                _packs.Add(_service.GetPackById(pack.Id));
            }
        }

        private static string[] GetWords(string input)
        {
            var matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                where !string.IsNullOrEmpty(m.Value)
                select TrimSuffix(m.Value);

            return words.ToArray();
        }

        private static string TrimSuffix(string word)
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
