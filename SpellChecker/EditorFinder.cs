using Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    public class EditorFinder
    {
        public void Run(List<Pack> packs)
        {
            var lines = new List<string>() { "Author,Name,Phrase,Complexity,Description,State" };
            foreach(var pack in packs)
            {
                foreach(var phrase in pack.Phrases)
                {
                    foreach(var reviewer in phrase.ReviewerObjects)
                    {
                        if(reviewer.ReviewState == State.Edit)
                        {
                            var line = $"{reviewer.Author},{pack.Name},{phrase.Phrase},{phrase.Complexity},{phrase.Description},{reviewer.ReviewState};";
                            lines.Add(line);
                        }
                    }
                }
            }

            //var path = "C:\\test.xlsx";
            
            //var f = new FileInfo(path);
            //var excelPackage = new ExcelPackage(f);




            //File.WriteAllLines(path, lines, Encoding.UTF8);
            //Console.WriteLine($"Всего на редактирование: {lines.Count}");
        }
    }
}
