using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Model;
using NUnit.Framework;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.InputDevices;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.TreeItems;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.WPFUIItems;
using TestStack.White.WindowsAPI;
using Debug = System.Diagnostics.Debug;

namespace UITest
{
    public class UITests
    {
        protected const int StartupTimeOut = 1000;
        private const int TestPackId = 20;
        private static readonly string ExePath;
        private static readonly Random Random = new Random();
        private readonly IPackService _packService = new PackService();
        private readonly string _testAuthor = "zhadko";

        static UITests()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codeBase = new Uri(assembly.CodeBase);
            var path = codeBase.LocalPath;
            var directoryInfo = new DirectoryInfo(path);
            var root = directoryInfo.Parent?.Parent?.Parent?.Parent?.FullName;

            var outputPath = root + @"\HatNewUI\bin\Debug";
            ExePath = outputPath + @"\HatNewUI.exe";
        }

        private Window MainWindow { get; set; }

        /// <summary>
        ///     Method executed before the tests start
        /// </summary>
        [SetUp]
        public void Startup()
        {
            App = Application.AttachOrLaunch(new ProcessStartInfo(ExePath));
            Thread.Sleep(1000);
            MainWindow = App.GetWindow(SearchCriteria.ByText("HAT DESKTOP"), InitializeOption.NoCache);
        }

        public Application App { get; set; }

        [TearDown]
        public void TearDown()
        {
            MainWindow?.Close();
            App.Close();
        }

        [Test]
        public void CorrectLaunch()
        {
            var window = MainWindow;
            Assert.IsNotNull(window);
        }

        [Test]
        public void SelectPack20_CorrectPhraseLoading()
        {
            var phraseItem = GeneratePhrase();
            _packService.AddPhrase(TestPackId, phraseItem);

            SelectFirstPack();
            Thread.Sleep(1000);
            SelectLastPack();
            Thread.Sleep(1000);

            AssertPhraseExistInGrid(phraseItem);

            DeletePhrase(phraseItem);
        }

        private void AssertPhraseExistInGrid(PhraseItem phraseItem)
        {
            var rows = GetItemsFromListView(GetPhraseGrid());
            var row = rows.FirstOrDefault(r => r.Cells[0].Text == phraseItem.Phrase);
            Assert.IsNotNull(row);
            Assert.IsTrue(row.Cells[1].Text == phraseItem.Complexity.ToString(CultureInfo.CurrentCulture));
            Assert.IsTrue(row.Cells[2].Text == phraseItem.Description);
        }

        private static PhraseItem GeneratePhrase()
        {
            return new PhraseItem {Phrase = RandomString(15), Description = RandomString(50), Complexity = Random.Next(1, 5)};
        }

        [Test]
        public void SelectPack1_CorrectDescriptionRepresentation()
        {
            var pack = _packService.GetPackById(1);
            SelectFirstPack();

            var description = MainWindow.Get<Label>(SearchCriteria.ByAutomationId("PackDescriptionLabel")).Text;

            Assert.AreEqual(pack.Description, description);
        }

        [Test]
        public void RefreshPack_CreatedPhraseAppeared()
        {
            SelectLastPack();
            var phrase = GeneratePhrase();
            AddPhrase(phrase);

            MainWindow.Get<Button>(SearchCriteria.ByAutomationId("RefreshButton"));
            Thread.Sleep(1000);

            AssertPhraseExistInGrid(phrase);

            DeletePhrase(phrase);
        }

        [Test]
        public void AddNewPhrase_SuccessAdded()
        {
            SelectLastPack();
            var phrase = new PhraseItem();
            ClickNewItem();
            Keyboard.Instance.Enter(phrase.Phrase);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.TAB);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.TAB);
            Keyboard.Instance.Enter(phrase.Description);
            ClickButton("SavePhraseButton");
            AssertPhraseExistInGrid(phrase);

            DeletePhrase(phrase);
        }

        [Test]
        public void AddDuplicatedPhrase_PhraseWasNotAdded()
        {
            var phrase = new PhraseItem();
            AddPhrase(phrase);
            SelectFirstPack();
            Thread.Sleep(1000);
            SelectLastPack();
            Thread.Sleep(1000);
            ClickNewItem();
            Keyboard.Instance.Enter(phrase.Phrase);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.TAB);
            Keyboard.Instance.PressSpecialKey(KeyboardInput.SpecialKeys.TAB);
            Keyboard.Instance.Enter(phrase.Description);
            ClickButton("SavePhraseButton");

            var warningWindow = MainWindow.Get<Window>(SearchCriteria.ByText("Warning"));

            Assert.IsNotNull(warningWindow);

            var rows = GetItemsFromListView(GetPhraseGrid());
            var row = rows.FirstOrDefault(r => r.Cells[0].Text == phrase.Phrase);
            Assert.IsNull(row);

            DeletePhrase(phrase);
        }

        private void ClickButton(string buttonAutomationId)
        {
            MainWindow.Get<Button>(SearchCriteria.ByAutomationId(buttonAutomationId)).Click();
        }

        private void DeletePhrase(PhraseItem phrase)
        {
            _packService.DeletePhrase(TestPackId, phrase.Phrase, _testAuthor);
        }

        private void AddPhrase(PhraseItem phrase)
        {
            _packService.AddPhrase(TestPackId, phrase);
        }

        private static IEnumerable<ListViewRow> GetItemsFromListView(ListView dataGrid)
        {
            var result = dataGrid.Rows;

            if (dataGrid.ScrollBars.Vertical.Value < 0)
            {
                return result;
            }

            do
            {
                Debug.WriteLine($"Value: {dataGrid.ScrollBars.Vertical.Value}");
                Debug.WriteLine($"Maximum Value: {dataGrid.ScrollBars.Vertical.MaximumValue}");
                dataGrid.ScrollBars.Vertical.ScrollDown();
                result.AddRange(dataGrid.Rows);
            } while (dataGrid.ScrollBars.Vertical.Value < dataGrid.ScrollBars.Vertical.MaximumValue);

            return result.Distinct().ToList();
        }

        private List<ListItem> GetItemsFromCombobox(ComboBox comboBox)
        {
            var result = comboBox.Items.ToList();

            if (comboBox.ScrollBars.Vertical.Value < 0)
            {
                return result;
            }

            do
            {
                Debug.WriteLine($"Value: {comboBox.ScrollBars.Vertical.Value}");
                Debug.WriteLine($"Maximum Value: {comboBox.ScrollBars.Vertical.MaximumValue}");
                comboBox.ScrollBars.Vertical.ScrollDown();
                result.AddRange(comboBox.Items);
            } while (comboBox.ScrollBars.Vertical.Value < comboBox.ScrollBars.Vertical.MaximumValue);

            return result.ToList().Distinct().ToList();
        }

        private void SelectLastPack()
        {
            var comboBox = MainWindow.Get<ComboBox>(SearchCriteria.ByAutomationId("PackCombobox"));
            var items = GetItemsFromCombobox(comboBox);
            items.Last().Click();
        }

        private void SelectFirstPack()
        {
            var pack = _packService.GetPackById(1);
            MainWindow.Get<ComboBox>(SearchCriteria.ByAutomationId("PackCombobox")).Select(pack.WholeName);
        }

        private ListView GetPhraseGrid()
        {
            return MainWindow.Get<ListView>(SearchCriteria.ByAutomationId("PhraseDataGrid"));
        }

        private static string RandomString(int length, string prefix = "")
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var randomString = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
            return prefix + randomString;
        }

        private void ClickNewItem()
        {
            //This doesn't work. The app can't find New Item button at all
            //MainWindow.Get<Button>(SearchCriteria.ByAutomationId("NewItemButton")).Click();

            var point = MainWindow.Get<Label>(SearchCriteria.ByText("PHRASE")).Location;
            //DrawMouse(point, new Point(point.X + 10, point.Y + 40), 500);
            Mouse.Instance.Click(new Point(point.X + 10, point.Y + 40));
            MainWindow.WaitWhileBusy();

            //TODO: try to find a best way to find "New Item" button
        }
    }
}