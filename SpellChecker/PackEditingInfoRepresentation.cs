using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    public class PackEditingInfoRepresentation
    {
        public void Run()
        {
            var service = new PackService();
            var packDictionary = service.GetAllPacksInfo().ToDictionary(x => x.Id, x => x.Name);
            var infoList = service.GetPackEditingInfo(packDictionary);

            infoList = infoList.Where(p => p.Pack != 20).ToList();
            var groupedByUser = infoList.GroupBy(i => i.Author).ToList();

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("20 пак пропускаю, не ссать!");
            Console.WriteLine("Ревью за все время (с того момент как фома соизволил написать стату):");
            ShowAllInfo(groupedByUser);

            Console.WriteLine("Ревью за последние 5 дней:");
            ShowAllInfo(groupedByUser, DateTime.Now.AddDays(-5));

            Console.WriteLine("Ревью за последние сутки");
            ShowAllInfo(groupedByUser, DateTime.Now.AddDays(-1));

            ShowNothigStat(infoList, groupedByUser);

            Console.ReadKey();
        }

        private static void ShowNothigStat(List<PhraseEditInfo> infoList, List<IGrouping<string, PhraseEditInfo>> groupedByUser)
        {
            DateTime startDate = infoList.Min(i => i.Date);
            Console.WriteLine($"Дней ватокатства начиная с {startDate.ToString("dd-MMMM-yyyy")} ({Math.Round((DateTime.Now - startDate).TotalDays, MidpointRounding.ToEven)} дней):");
            var dates = new List<DateTime>();
            for (var day = startDate; day.Date <= DateTime.Now; day = day.AddDays(1))
            {
                dates.Add(day);
            }

            foreach (var info in groupedByUser)
            {
                var uniqueDates = info.Select(i => new DateTime(i.Date.Year, i.Date.Month, i.Date.Day)).Distinct().ToList();
                var count = dates.Count - uniqueDates.Count;

                Console.Write($"{info.Key}: {count}\n");
            }
        }

        private static void ShowAllInfo(IEnumerable<IGrouping<string, PhraseEditInfo>> groupedByUser, DateTime startDate = default(DateTime))
        {
            foreach (var info in groupedByUser)
            {
                Console.WriteLine($"{info.Key}:");
                WriteCountPerType(info, "adddesc", startDate);
                WriteCountPerType(info, "edit", startDate);
                WriteCountPerType(info, "review", startDate);
                WriteCountPerType(info, "remove", startDate);
                Console.WriteLine();
            }
        }

        private static void WriteCountPerType(IGrouping<string, PhraseEditInfo> info, string type, DateTime startDate)
        {
            Console.Write($"{type}: ");
            Console.Write($"{info.Count(i => i.Type == type && i.Date > startDate)}\n");
        }
    }
}
