using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Model;
using NHunspell;

namespace SpellChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Run();
        }

        private static async Task Run()
        {
            var service = new PackService();
            Console.WriteLine("Data loading...");
            var packs = await service.GetFullPacksData(8081);
            Console.WriteLine("Data loading is completed");

            using (var hunspell = new Hunspell("ru_ru.aff", "ru_ru.dic"))
            {
                foreach (var pack in packs)
                {
                    foreach (var phrase in pack.Phrases)
                    {
                        foreach (var word in GetWords(phrase.Phrase))
                        {
                            if (!hunspell.Spell(word))
                            {
                                Console.WriteLine($"Ошибка в слове {word}");
                                Console.ReadKey();
                            }
                        }

                        foreach (var word in GetWords(phrase.Description))
                        {
                            if (!hunspell.Spell(word))
                            {
                                Console.WriteLine($"Ошибка в слове {word}");
                                Console.ReadKey();
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Spellcheck is finished");
            Console.ReadKey();
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
