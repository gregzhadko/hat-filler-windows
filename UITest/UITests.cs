using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace UITest
{
    public class UITests
    {
        protected static string OutputPath;
        protected static string ExePath;
        protected const int StartupTimeOut = 1000;
        protected Application App;
        private static readonly Random Random = new Random();

        static UITests()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codeBase = new Uri(assembly.CodeBase);
            var path = codeBase.LocalPath;
            var directoryInfo = new DirectoryInfo(path);
            var root = directoryInfo.Parent?.Parent?.Parent?.Parent?.FullName;

            OutputPath = root + @"\HatNewUI\bin\Debug";
            ExePath = OutputPath + @"\HatNewUI.exe";
        }

        /// <summary>
        ///     Method executed before the tests start
        /// </summary>
        [SetUp]
        public void Startup()
        {
            App = Application.AttachOrLaunch(new ProcessStartInfo(ExePath));
        }

        [TearDown]
        public void TearDown()
        {
            App.Close();
        }

        [Test]
        public void CorrectLaunch()
        {
            var window = GetMainWindow;
            Assert.IsNotNull(window);
        }

        [Test]
        public void SelectPack20()
        {
            var window = GetMainWindow;
            var comboBox = window.Get<ComboBox>(SearchCriteria.ByAutomationId("PackCombobox"));
            comboBox.Select(comboBox.Items.First().Text);
            comboBox.Select(comboBox.Items.Last().Text);
        }

        public Window GetMainWindow => App.GetWindow(SearchCriteria.ByText("HAT DESKTOP"), InitializeOption.NoCache);

        protected static string RandomString(int length, string prefix)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var randomString = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
            return prefix + randomString;
        }
    }
}
