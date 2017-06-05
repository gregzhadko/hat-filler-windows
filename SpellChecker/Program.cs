using System;

namespace SpellChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var spellChecker = new SpellChecker();
            //spellChecker.Run();

            //var localizationChecker = new LocalizationChecker();
            //localizationChecker.Run(@"D:\Repo\hat-swift\Hat\Hat\Resources\en.lproj\Localizable.strings", @"D:\result.txt");

            var editingInfo = new PackEditingInfoRepresentation();
            editingInfo.Run();

            Console.ReadKey();
        }
    }
}
