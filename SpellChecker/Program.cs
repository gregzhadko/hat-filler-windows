using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpellChecker
{
    public class Program
    {
        private static readonly PackService _service = new PackService();
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Загружаю паки...");
            var packs = LoadPacks();
            Console.WriteLine("Загрузка паков завершена");

            var spellChecker = new SpellChecker(packs);
            spellChecker.Run();

            //ReviewAllPacks(packs);

            Console.ReadKey();
        }

        private static void ReviewAllPacks(List<Pack> packs)
        {
            var authors = Reviewer.NewReviewers().Select(r => r.Author).ToList();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var pack in packs.Where(p => p.Id == 13 || p.Id == 9 || p.Id == 10 || p.Id == 11))
            {
                Console.WriteLine($"Смотрю пак {pack.Name}");
                foreach (var phrase in pack.Phrases)
                {
                    Console.WriteLine($"Cмотрю фразу {phrase.Phrase}");
                    foreach (var author in authors)
                    {
                        _service.ReviewPhrase(pack.Id, phrase, author, State.Accept);
                    }
                }
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }

        private static List<Pack> LoadPacks()
        {
            var packs = _service.GetAllPacksInfo();
            var result = new List<Pack>();
            foreach (var pack in packs)
            {
                Console.WriteLine($"Загрузка пака {pack.Name}");
                result.Add(_service.GetPackById(pack.Id));
            }

            return result;
        }
    }
}
