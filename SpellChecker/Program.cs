using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpellChecker
{
    public class Program
    {
        private static PackService _service = new PackService();
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Загружаю паки...");
            var packs = LoadPacks();
            Console.WriteLine("Загрузка паков завершена");

            //var lines = File.ReadAllLines(@"C:\Repo\Hat\HatDesktop\SpellChecker\CustomDictionary.txt");
            //var uniqueLines = lines.Distinct().ToList();
            //File.WriteAllLines(@"C:\Repo\Hat\HatDesktop\SpellChecker\CustomDictionary.txt", uniqueLines);

            var spellChecker = new SpellChecker(packs);
            spellChecker.Run();

            //var localizationChecker = new LocalizationChecker();
            //localizationChecker.Run(@"D:\Repo\hat-swift\Hat\Hat\Resources\en.lproj\Localizable.strings", @"D:\result.txt");

            var editorFinder = new EditorFinder();
            editorFinder.Run(packs);

            var editingInfo = new PackEditingInfoRepresentation();
            editingInfo.Run();

            Console.ReadKey();
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
