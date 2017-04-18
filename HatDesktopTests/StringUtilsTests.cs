using HatDesktop;
using NUnit.Framework;

namespace HatDesktopTests
{
    [TestFixture]
    public class StringUtilsTests
    {
        [Test]
        public void ReplaceFirstCharToUpperTest()
        {
            Assert.That(StringUtils.ReplaceFirstCharToUpper(null), Is.Null);
            Assert.That(string.Empty.ReplaceFirstCharToUpper(), Is.Empty);
            Assert.That("aaaa aaaa".ReplaceFirstCharToUpper(), Is.EqualTo("Aaaa aaaa"));
            Assert.That("Aaaa aaaa".ReplaceFirstCharToUpper(), Is.EqualTo("Aaaa aaaa"));
        }

        [Test]
        public void ReplaceQuotesTest()
        {
            Assert.That(StringUtils.ReplaceQuotes(null), Is.Null);
            Assert.That(string.Empty.ReplaceQuotes(), Is.Empty);

            Assert.That("Test".ReplaceQuotes(), Is.EqualTo("Test"));
            Assert.That("\"Test\"".ReplaceQuotes(), Is.EqualTo("«Test»"));
            Assert.That("123 \"Test\" 321".ReplaceQuotes(), Is.EqualTo("123 «Test» 321"));
            Assert.That("   \"Test\"   ".ReplaceQuotes(), Is.EqualTo("   «Test»   "));
        }

        [Test]
        public void ReplaceHyphenWithDashTest()
        {
            Assert.That(StringUtils.ReplaceHyphenWithDash(null), Is.Null);
            Assert.That(string.Empty.ReplaceHyphenWithDash(), Is.Empty);
            Assert.That("aaaa-aaaa".ReplaceHyphenWithDash(), Is.EqualTo("aaaa-aaaa"));
            Assert.That("Aaaa - aaaa".ReplaceHyphenWithDash(), Is.EqualTo("Aaaa — aaaa"));
        }

        [Test]
        public void RemoveSquareBracketsTest()
        {
            Assert.That(StringUtils.RemoveSquareBrackets(null), Is.Null);
            Assert.That(string.Empty.RemoveSquareBrackets(), Is.Empty);
            Assert.That("aaaa[1]".RemoveSquareBrackets(), Is.EqualTo("aaaa"));
            Assert.That("aaaa [1]".RemoveSquareBrackets(), Is.EqualTo("aaaa "));
            Assert.That("aaaa [1] bbbb".RemoveSquareBrackets(), Is.EqualTo("aaaa bbbb"));
            Assert.That("aaaa [1] bbbb [2]".RemoveSquareBrackets(), Is.EqualTo("aaaa bbbb "));
            Assert.That("aaaa [1] bbbb [2]cccc".RemoveSquareBrackets(), Is.EqualTo("aaaa bbbb cccc"));

        }

        [Test]
        public void RemoveMultipleSpacesTest()
        {
            Assert.That(StringUtils.RemoveMultipleSpaces(null), Is.Null);
            Assert.That(string.Empty.RemoveMultipleSpaces(), Is.Empty);
            Assert.That("aa   a".RemoveMultipleSpaces(), Is.EqualTo("aa a"));
            Assert.That("aa   a   ".RemoveMultipleSpaces(), Is.EqualTo("aa a "));
            Assert.That("   aa   a   ".RemoveMultipleSpaces(), Is.EqualTo(" aa a "));
        }

        [Test]
        public void AddDotTest()
        {
            Assert.That("aaa".AddDot(), Is.EqualTo("aaa."));
            Assert.That("aaa.".AddDot(), Is.EqualTo("aaa."));
        }

        [Test]
        [TestCase("test [1] - [2]   \"test\" [4]", "Test — «test».")]
        [TestCase("ыаываываыв; ываыв аыв.", "Ыаываываыв; ываыв аыв.")]
        public void FormatDescriptionTest(string input, string result)
        {
            Assert.That(StringUtils.FormatDescription(input), Is.EqualTo(result));
        }
    }
}