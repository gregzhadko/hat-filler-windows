using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Model;
using NHunspell;
using Yandex.Speller.Api;
using Yandex.Speller.Api.DataContract;

namespace SpellChecker
{
    public class SpellChecker
    {
        
        private readonly List<Pack> _packs;
        private readonly List<string> _skippedPhrases = File.ReadAllLines("SkipDictionary.txt").ToList();

        public SpellChecker(List<Pack> packs)
        {
            _packs = packs;
        }

        public void Run()
        {
            var yandexSpellCheck = new YandexSpeller();

            using (var hunSpellEng = new Hunspell(@"Dictionaries\English (American).aff", @"Dictionaries\English (American).dic"))
            using (var hunSpell = new Hunspell(@"Dictionaries\Russian.aff", @"Dictionaries\Russian.dic"))
            {
                InitCustomDictionary(hunSpell);

                //Skip test pack with id 20
                foreach (var pack in _packs.Where(p => p.Id != 20))
                {
                    Console.WriteLine($"Смотрю пак {pack.Name}");
                    foreach (var phrase in pack.Phrases)
                    {
                        SpellPhrase(pack, phrase.Phrase, hunSpell, hunSpellEng, yandexSpellCheck);
                        SpellPhrase(pack, phrase.Description, hunSpell, hunSpellEng, yandexSpellCheck);
                    }
                }
            }
            Console.WriteLine("Spell check is finished");
        }

        private void InitCustomDictionary(Hunspell hunSpell)
        {
            var customWords = File.ReadAllLines("CustomDictionary.txt");
            foreach (var line in customWords)
            {
                hunSpell.Add(line);
            }
        }

        private void SpellPhrase(Pack pack, string phrase, Hunspell hunSpell, Hunspell hunSpellEng, IYandexSpeller speller)
        {
            var words = GetWords(phrase);
            foreach (var word in Enumerable.Select(words, w => w.ToLowerInvariant().Replace('ё', 'е')))
            {
                if (hunSpell.Spell(word) || hunSpellEng.Spell(word) || ExistsInSkipped(word, phrase, pack.Id))
                {
                    continue;
                }
                if (!YandexSpellCheckPass(speller, word))
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

        private bool YandexSpellCheckPass(IYandexSpeller speller, string word)
        {
            return !speller.CheckText(word, Lang.Ru | Lang.En, Options.IgnoreCapitalization, TextFormat.Plain).Errors.Any();
        }

        private void ShowSpellErrorMessages(Pack pack, string phrase, string word)
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

        private bool ExistsInSkipped(string word, string wholeWord, int id)
        {
            return _skippedPhrases.Select(s => s.Split('|')).Any(line => string.Compare(line[0], word, StringComparison.OrdinalIgnoreCase) == 0 &&
                                                                                string.Compare(line[1], id.ToString(), StringComparison.OrdinalIgnoreCase) == 0 &&
                                                                                string.Compare(line[2], wholeWord, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private void SaveNewCustomWord(Hunspell hunSpell, string word)
        {
            hunSpell.Add(word);
            File.AppendAllLines(@"..\..\CustomDictionary.txt", new[] {word});
            File.AppendAllLines(@"CustomDictionary.txt", new[] {word});
            Console.WriteLine($"\nСлово {word} было добавлено в персональный словарь");
        }

        private void SaveNewSkipWord(string word, string wholeWord, int packId)
        {
            var stringToSave = $"{word}|{packId}|{wholeWord}";
            File.AppendAllLines(@"..\..\SkipDictionary.txt", new[] {stringToSave});
            File.AppendAllLines(@"SkipDictionary.txt", new[] {stringToSave});
            Console.WriteLine($"\nСлово {word} было добавлено в словарь пропущенных слов");
        }

        private IEnumerable<string> GetWords(string input)
        {
            var matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                where !string.IsNullOrEmpty(m.Value)
                select TrimSuffix(m.Value);

            return words.ToArray();
        }

        private string TrimSuffix(string word)
        {
            var apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}