using System.Collections.Generic;
using NUnit.Framework;
using Xeon.UniTerminal.Common;

namespace Xeon.UniTerminal.Tests
{
    public class TextMeshUtilityTests
    {
        [Test]
        public void WrapText_WhenWithinLimit_ReturnsOriginal()
        {
            var result = TextMeshUtility.WrapText("terminal", 10);

            Assert.That(result, Is.EqualTo(new List<string> { "terminal" }));
        }

        [Test]
        public void WrapText_WithSpaces_BreaksAtWordBoundaries()
        {
            var result = TextMeshUtility.WrapText("hello world from unity", 7);

            Assert.That(result, Is.EqualTo(new List<string>
            {
                "hello",
                "world",
                "from",
                "unity",
            }));
        }

        [Test]
        public void WrapText_WithoutSpaces_ForcesSplitByLimit()
        {
            var result = TextMeshUtility.WrapText("abcdefghij", 4);

            Assert.That(result, Is.EqualTo(new List<string>
            {
                "abcd",
                "efgh",
                "ij",
            }));
        }

        [Test]
        public void WrapText_WithNonPositiveLimit_ReturnsSingleLine()
        {
            var result = TextMeshUtility.WrapText("sample", 0);

            Assert.That(result, Is.EqualTo(new List<string> { "sample" }));
        }
    }
}
