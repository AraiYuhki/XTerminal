using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Xeon.XTerminal.Tests
{
    public class VariableCommandTests
    {
        private Terminal terminal;
        private ListTextWriter stdout;
        private ListTextWriter stderr;

        [SetUp]
        public void SetUp()
        {
            terminal = new Terminal(Application.temporaryCachePath, Application.temporaryCachePath);
            stdout = new ListTextWriter();
            stderr = new ListTextWriter();
        }

        [TearDown]
        public void TearDown()
        {
            terminal?.Dispose();
        }

        [Test]
        public async Task SetCommand_ValidAssignment_SetsVariable()
        {
            var exitCode = await terminal.ExecuteAsync("set FOO=bar", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual("bar", terminal.Variables.Get("FOO"));
        }

        [Test]
        public async Task SetCommand_EmptyValue_SetsEmptyString()
        {
            var exitCode = await terminal.ExecuteAsync("set FOO=", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual("", terminal.Variables.Get("FOO"));
        }

        [Test]
        public async Task SetCommand_ValueWithSpaces_SetsCorrectly()
        {
            var exitCode = await terminal.ExecuteAsync("set FOO=hello world", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual("hello world", terminal.Variables.Get("FOO"));
        }

        [Test]
        public async Task SetCommand_NoEquals_ReturnsUsageError()
        {
            var exitCode = await terminal.ExecuteAsync("set FOO", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.Lines.Count > 0);
        }

        [Test]
        public async Task SetCommand_InvalidName_ReturnsUsageError()
        {
            var exitCode = await terminal.ExecuteAsync("set 123=value", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
        }

        [Test]
        public async Task SetCommand_NoArguments_ReturnsUsageError()
        {
            var exitCode = await terminal.ExecuteAsync("set", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
        }

        [Test]
        public async Task UnsetCommand_ExistingVariable_RemovesVariable()
        {
            terminal.Variables.Set("FOO", "bar");

            var exitCode = await terminal.ExecuteAsync("unset FOO", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual("", terminal.Variables.Get("FOO"));
        }

        [Test]
        public async Task UnsetCommand_NonexistentVariable_Succeeds()
        {
            var exitCode = await terminal.ExecuteAsync("unset NONEXISTENT", stdout, stderr);
            Assert.AreEqual(ExitCode.Success, exitCode);
        }

        [Test]
        public async Task UnsetCommand_MultipleVariables_RemovesAll()
        {
            terminal.Variables.Set("A", "1");
            terminal.Variables.Set("B", "2");
            terminal.Variables.Set("C", "3");

            var exitCode = await terminal.ExecuteAsync("unset A B C", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual("", terminal.Variables.Get("A"));
            Assert.AreEqual("", terminal.Variables.Get("B"));
            Assert.AreEqual("", terminal.Variables.Get("C"));
        }

        [Test]
        public async Task UnsetCommand_NoArguments_ReturnsUsageError()
        {
            var exitCode = await terminal.ExecuteAsync("unset", stdout, stderr);
            Assert.AreEqual(ExitCode.UsageError, exitCode);
        }

        [Test]
        public async Task EnvCommand_NoVariables_OutputsNothing()
        {
            terminal.Variables.Clear();

            var exitCode = await terminal.ExecuteAsync("env", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(0, stdout.Lines.Count);
        }

        [Test]
        public async Task EnvCommand_WithVariables_OutputsSortedList()
        {
            terminal.Variables.Clear();
            terminal.Variables.Set("ZZZ", "3");
            terminal.Variables.Set("AAA", "1");
            terminal.Variables.Set("MMM", "2");

            var exitCode = await terminal.ExecuteAsync("env", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(3, stdout.Lines.Count);
            Assert.AreEqual("AAA=1", stdout.Lines[0]);
            Assert.AreEqual("MMM=2", stdout.Lines[1]);
            Assert.AreEqual("ZZZ=3", stdout.Lines[2]);
        }

        [Test]
        public async Task SetAndEcho_VariableIsExpanded()
        {
            await terminal.ExecuteAsync("set MESSAGE=Hello", stdout, stderr);
            stdout.Clear();

            var exitCode = await terminal.ExecuteAsync("echo $MESSAGE", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(1, stdout.Lines.Count);
            Assert.AreEqual("Hello", stdout.Lines[0]);
        }

        [Test]
        public async Task SetUnsetAndEcho_VariableIsEmpty()
        {
            await terminal.ExecuteAsync("set MESSAGE=Hello", stdout, stderr);
            await terminal.ExecuteAsync("unset MESSAGE", stdout, stderr);
            stdout.Clear();

            var exitCode = await terminal.ExecuteAsync("echo $MESSAGE", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(1, stdout.Lines.Count);
            Assert.AreEqual("", stdout.Lines[0]);
        }

        [Test]
        public async Task SetAndEnv_ShowsSetVariable()
        {
            terminal.Variables.Clear();
            await terminal.ExecuteAsync("set AAA=123", stdout, stderr);
            stdout.Clear();

            var exitCode = await terminal.ExecuteAsync("env", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(1, stdout.Lines.Count);
            Assert.AreEqual("AAA=123", stdout.Lines[0]);
        }

        [Test]
        public async Task SetUnsetAndEnv_DoesNotShowVariable()
        {
            terminal.Variables.Clear();
            await terminal.ExecuteAsync("set AAA=123", stdout, stderr);
            await terminal.ExecuteAsync("unset AAA", stdout, stderr);
            stdout.Clear();

            var exitCode = await terminal.ExecuteAsync("env", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual(0, stdout.Lines.Count);
        }

    }
}
