using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    internal class LocalizationChecker
    {
        internal void Run(string inputFile, string outputFile)
        {
            var lines = File.ReadAllLines(inputFile).Where(l => l.Contains('='));
            var result = new List<string>();
            foreach (var line in lines)
            {
                var index1 = line.IndexOf('\"');
                var index2 = line.IndexOf('\"', index1+1);
                var index3 = line.IndexOf('\"', index2+1);
                var s = line.Substring(index3 + 1, line.Length - index3 - 3);
                s = s.Replace(@"\n", " ");
                s = s.Replace(@"\n1", " ");
                s = s.Replace(@"\n2", " ");
                s = s.Replace(@"%d", "");
                s = s.Replace(@"%@", "");
                result.Add(s);
            }

            File.Delete(outputFile);
            File.WriteAllLines(outputFile, result);
        }
    }
}
