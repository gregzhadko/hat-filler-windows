using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpellChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var lines = File.ReadAllLines(@"C:\Repo\Hat\HatDesktop\SpellChecker\CustomDictionary.txt");
            //var uniqueLines = lines.Distinct().ToList();
            //File.WriteAllLines(@"C:\Repo\Hat\HatDesktop\SpellChecker\CustomDictionary.txt", uniqueLines);


            var spellChecker = new SpellChecker();
            spellChecker.Run();

            //var localizationChecker = new LocalizationChecker();
            //localizationChecker.Run(@"D:\Repo\hat-swift\Hat\Hat\Resources\en.lproj\Localizable.strings", @"D:\result.txt");

            var editingInfo = new PackEditingInfoRepresentation();
            editingInfo.Run();

            Console.ReadKey();
        }
    }
}
