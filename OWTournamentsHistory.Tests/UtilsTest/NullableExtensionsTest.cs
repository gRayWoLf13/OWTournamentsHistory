using OWTournamentsHistory.Common.Utils;
using System.Globalization;

namespace OWTournamentsHistory.Test.UtilsTest
{
    public class NullableExtensionsTest
    {
        [Test]
        [TestCase("123")]
        [TestCase("0123")]
        [TestCase("-1")]
        public void TestIntParsing(string value)
        {
            var parsed = value.ParseTo<int>();
            Assert.Multiple(() =>
            {
                Assert.That(parsed, Is.Not.Null);
                Assert.That(value.ParseTo<int>()!.Value, Is.EqualTo(int.Parse(value)));
            });
        }

        [Test]
        [TestCase("123")]
        [TestCase("")]
        public void TestIntNullableParsing(string value)
        {
            var parsed = value.ParseTo<int>();
            if (string.IsNullOrEmpty(value))
            {
                Assert.That(parsed, Is.Null);
            }
            else
            {
                Assert.Multiple(() =>
                {
                    Assert.That(parsed, Is.Not.Null);
                    Assert.That(value.ParseTo<int>()!.Value, Is.EqualTo(int.Parse(value)));
                });
            }
        }

        [Test]
        [TestCase("123")]
        [TestCase("123.321")]
        public void TestDecimalParsing(string value)
        {
            var parsed = value.ParseTo<decimal>();
            Assert.Multiple(() =>
            {
                Assert.That(parsed, Is.Not.Null);
                Assert.That(value.ParseTo<decimal>()!.Value, Is.EqualTo(decimal.Parse(value, CultureInfo.InvariantCulture)));
            });
        }

        [Test]
        [TestCase("123.321")]
        [TestCase("")]
        public void TestDecimalNullableParsing(string value)
        {
            var parsed = value.ParseTo<decimal>();
            if (string.IsNullOrEmpty(value))
            {
                Assert.That(parsed, Is.Null);
            }
            else
            {
                Assert.Multiple(() =>
                {
                    Assert.That(parsed, Is.Not.Null);
                    Assert.That(value.ParseTo<decimal>()!.Value, Is.EqualTo(decimal.Parse(value, CultureInfo.InvariantCulture)));
                });
            }
        }

        [Test]
        public void TestInvalidFormat()
        {
            Assert.Throws<FormatException>(() => { "   ".ParseTo<int>(); });
            Assert.Throws<FormatException>(() => { "   ".ParseTo<decimal>(); });
        }
    }
}
