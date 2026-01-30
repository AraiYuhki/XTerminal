using NUnit.Framework;
using Xeon.UniTerminal.Parsing;

namespace Xeon.UniTerminal.Tests
{
    public class VariableTests
    {
        private VariableStore store;
        private Parser parser;

        [SetUp]
        public void SetUp()
        {
            store = new VariableStore();
            parser = new Parser();
        }

        [Test]
        public void VariableStore_SetAndGet_ReturnsValue()
        {
            store.Set("FOO", "bar");
            Assert.AreEqual("bar", store.Get("FOO"));
        }

        [Test]
        public void VariableStore_GetUndefined_ReturnsEmptyString()
        {
            Assert.AreEqual("", store.Get("UNDEFINED"));
        }

        [Test]
        public void VariableStore_Unset_RemovesVariable()
        {
            store.Set("FOO", "bar");
            Assert.IsTrue(store.Unset("FOO"));
            Assert.AreEqual("", store.Get("FOO"));
        }

        [Test]
        public void VariableStore_UnsetUndefined_ReturnsFalse()
        {
            Assert.IsFalse(store.Unset("UNDEFINED"));
        }

        [Test]
        public void VariableStore_Enumerate_ReturnsSortedList()
        {
            store.Set("ZZZ", "1");
            store.Set("AAA", "2");
            store.Set("MMM", "3");

            var items = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>(store.Enumerate());

            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("AAA", items[0].Key);
            Assert.AreEqual("MMM", items[1].Key);
            Assert.AreEqual("ZZZ", items[2].Key);
        }

        [Test]
        public void VariableStore_IsValidName_AcceptsValidNames()
        {
            Assert.IsTrue(VariableStore.IsValidName("FOO"));
            Assert.IsTrue(VariableStore.IsValidName("_bar"));
            Assert.IsTrue(VariableStore.IsValidName("foo_bar"));
            Assert.IsTrue(VariableStore.IsValidName("FOO123"));
            Assert.IsTrue(VariableStore.IsValidName("_123"));
        }

        [Test]
        public void VariableStore_IsValidName_RejectsInvalidNames()
        {
            Assert.IsFalse(VariableStore.IsValidName(""));
            Assert.IsFalse(VariableStore.IsValidName(null));
            Assert.IsFalse(VariableStore.IsValidName("123"));
            Assert.IsFalse(VariableStore.IsValidName("foo-bar"));
            Assert.IsFalse(VariableStore.IsValidName("foo.bar"));
            Assert.IsFalse(VariableStore.IsValidName("foo bar"));
        }

        [Test]
        public void VariableStore_SetInvalidName_ThrowsException()
        {
            Assert.Throws<System.ArgumentException>(() => store.Set("123", "value"));
            Assert.Throws<System.ArgumentException>(() => store.Set("", "value"));
            Assert.Throws<System.ArgumentException>(() => store.Set("foo-bar", "value"));
        }

        [Test]
        public void Parse_UndefinedVariable_ExpandsToEmpty()
        {
            var result = parser.Parse("echo $UNDEFINED", store);
            Assert.AreEqual("echo", result.Pipeline.Commands[0].CommandName);
            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_DefinedVariable_ExpandsToValue()
        {
            store.Set("NAME", "hello");
            var result = parser.Parse("echo $NAME", store);

            Assert.AreEqual("echo", result.Pipeline.Commands[0].CommandName);
            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("hello", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_BracedVariable_ExpandsToValue()
        {
            store.Set("NAME", "world");
            var result = parser.Parse("echo ${NAME}", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("world", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_SingleQuotedVariable_NotExpanded()
        {
            store.Set("NAME", "hello");
            var result = parser.Parse("echo '$NAME'", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("$NAME", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_DoubleQuotedVariable_Expanded()
        {
            store.Set("NAME", "hello");
            var result = parser.Parse("echo \"$NAME\"", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("hello", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_ConcatenatedVariable_ExpandsCorrectly()
        {
            store.Set("BAR", "world");
            var result = parser.Parse("echo foo$BAR", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("fooworld", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_EscapedDollar_TreatedAsLiteral()
        {
            store.Set("NAME", "hello");
            var result = parser.Parse("echo \\$NAME", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("$NAME", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_EscapedDollarInDoubleQuotes_TreatedAsLiteral()
        {
            store.Set("NAME", "hello");
            var result = parser.Parse("echo \"\\$NAME\"", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("$NAME", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_MultipleVariables_AllExpanded()
        {
            store.Set("A", "hello");
            store.Set("B", "world");
            var result = parser.Parse("echo $A $B", store);

            Assert.AreEqual(2, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("hello", result.Pipeline.Commands[0].PositionalArguments[0]);
            Assert.AreEqual("world", result.Pipeline.Commands[0].PositionalArguments[1]);
        }

        [Test]
        public void Parse_MixedQuotesWithVariable_ExpandsCorrectly()
        {
            store.Set("VAR", "middle");
            var result = parser.Parse("echo 'start'$VAR\"end\"", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("startmiddleend", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_DollarAtEnd_TreatedAsLiteral()
        {
            var result = parser.Parse("echo test$", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("test$", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_VariableWithUnderscore_ExpandsCorrectly()
        {
            store.Set("MY_VAR", "value");
            var result = parser.Parse("echo $MY_VAR", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("value", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_VariableWithNumbers_ExpandsCorrectly()
        {
            store.Set("VAR123", "value");
            var result = parser.Parse("echo $VAR123", store);

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("value", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

        [Test]
        public void Parse_NoVariableStore_NoExpansion()
        {
            var result = parser.Parse("echo $NAME");

            Assert.AreEqual(1, result.Pipeline.Commands[0].PositionalArguments.Count);
            Assert.AreEqual("$NAME", result.Pipeline.Commands[0].PositionalArguments[0]);
        }

    }
}
