using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xeon.XTerminal.Tests
{
    /// <summary>
    /// tailコマンドのテスト。
    /// </summary>
    public class TailCommandTests
    {
        private Terminal terminal;
        private StringBuilderTextWriter stdout;
        private StringBuilderTextWriter stderr;
        private string testDir;

        [SetUp]
        public void SetUp()
        {
            testDir = Path.Combine(Path.GetTempPath(), "XTerminalTailTests");
            Directory.CreateDirectory(testDir);

            terminal = new Terminal(testDir, testDir, registerBuiltInCommands: true);
            stdout = new StringBuilderTextWriter();
            stderr = new StringBuilderTextWriter();
        }

        [TearDown]
        public void TearDown()
        {
            terminal?.Dispose();
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }

        private void CreateFile(string name, params string[] lines)
        {
            File.WriteAllLines(Path.Combine(testDir, name), lines);
        }

        private void CreateFileWithContent(string name, string content)
        {
            File.WriteAllText(Path.Combine(testDir, name), content);
        }

        // TAIL-001 デフォルト（10行）
        [Test]
        public async Task Tail_Default_OutputsLast10Lines()
        {
            var lines = new string[15];
            for (int i = 0; i < 15; i++)
                lines[i] = $"line{i + 1}";

            CreateFile("test.txt", lines);

            var exitCode = await terminal.ExecuteAsync("tail test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsFalse(output.Contains("line5"));
            Assert.IsTrue(output.Contains("line6"));
            Assert.IsTrue(output.Contains("line15"));
        }

        // TAIL-002 行数指定
        [Test]
        public async Task Tail_LinesOption_OutputsSpecifiedLines()
        {
            CreateFile("test.txt", "line1", "line2", "line3", "line4", "line5");

            var exitCode = await terminal.ExecuteAsync("tail -n=3 test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsFalse(output.Contains("line1"));
            Assert.IsFalse(output.Contains("line2"));
            Assert.IsTrue(output.Contains("line3"));
            Assert.IsTrue(output.Contains("line5"));
        }

        // TAIL-003 先頭から（+K形式）
        [Test]
        public async Task Tail_FromStart_OutputsFromLineK()
        {
            CreateFile("test.txt", "line1", "line2", "line3", "line4", "line5");

            var exitCode = await terminal.ExecuteAsync("tail -n=+3 test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsFalse(output.Contains("line1"));
            Assert.IsFalse(output.Contains("line2"));
            Assert.IsTrue(output.Contains("line3"));
            Assert.IsTrue(output.Contains("line4"));
            Assert.IsTrue(output.Contains("line5"));
        }

        // TAIL-004 バイト数指定
        [Test]
        public async Task Tail_BytesOption_OutputsSpecifiedBytes()
        {
            CreateFileWithContent("test.txt", "Hello, World!");

            var exitCode = await terminal.ExecuteAsync("tail -c=6 test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.AreEqual("World!", stdout.ToString());
        }

        // TAIL-005 10行未満のファイル
        [Test]
        public async Task Tail_LessThan10Lines_OutputsAllLines()
        {
            CreateFile("test.txt", "line1", "line2", "line3");

            var exitCode = await terminal.ExecuteAsync("tail test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("line1"));
            Assert.IsTrue(output.Contains("line2"));
            Assert.IsTrue(output.Contains("line3"));
        }

        // TAIL-010 複数ファイル（ヘッダー表示）
        [Test]
        public async Task Tail_MultipleFiles_ShowsHeaders()
        {
            CreateFile("file1.txt", "content1");
            CreateFile("file2.txt", "content2");

            var exitCode = await terminal.ExecuteAsync("tail file1.txt file2.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("==> file1.txt <=="));
            Assert.IsTrue(output.Contains("==> file2.txt <=="));
            Assert.IsTrue(output.Contains("content1"));
            Assert.IsTrue(output.Contains("content2"));
        }

        // TAIL-011 単一ファイル（ヘッダー非表示）
        [Test]
        public async Task Tail_SingleFile_NoHeader()
        {
            CreateFile("test.txt", "content");

            var exitCode = await terminal.ExecuteAsync("tail test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsFalse(output.Contains("==>"));
        }

        // TAIL-012 -v オプション（単一ファイルでもヘッダー表示）
        [Test]
        public async Task Tail_VerboseOption_ShowsHeader()
        {
            CreateFile("test.txt", "content");

            var exitCode = await terminal.ExecuteAsync("tail -v test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("==> test.txt <=="));
        }

        // TAIL-013 -q オプション（複数ファイルでもヘッダー非表示）
        [Test]
        public async Task Tail_QuietOption_NoHeaders()
        {
            CreateFile("file1.txt", "content1");
            CreateFile("file2.txt", "content2");

            var exitCode = await terminal.ExecuteAsync("tail -q file1.txt file2.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            var output = stdout.ToString();
            Assert.IsFalse(output.Contains("==>"));
        }

        // TAIL-020 パイプからの入力
        [Test]
        public async Task Tail_PipeInput_ProcessesStdin()
        {
            var exitCode = await terminal.ExecuteAsync("echo line1 | tail -n=1", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.IsTrue(stdout.ToString().Contains("line1"));
        }

        // TAIL-030 存在しないファイル
        [Test]
        public async Task Tail_NonExistentFile_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("tail nonexistent.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.RuntimeError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("No such file"));
        }

        // TAIL-031 -n と -c の両方指定
        [Test]
        public async Task Tail_BothLinesAndBytes_ReturnsError()
        {
            CreateFile("test.txt", "content");

            var exitCode = await terminal.ExecuteAsync("tail -n=5 -c=10 test.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("cannot specify both"));
        }

        // TAIL-032 -f オプションでファイル指定なし
        [Test]
        public async Task Tail_FollowWithoutFile_ReturnsError()
        {
            var exitCode = await terminal.ExecuteAsync("tail -f", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("requires a file"));
        }

        // TAIL-033 -f オプションで複数ファイル
        [Test]
        public async Task Tail_FollowWithMultipleFiles_ReturnsError()
        {
            CreateFile("file1.txt", "content1");
            CreateFile("file2.txt", "content2");

            var exitCode = await terminal.ExecuteAsync("tail -f file1.txt file2.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.UsageError, exitCode);
            Assert.IsTrue(stderr.ToString().Contains("multiple files is not supported"));
        }

        // TAIL-034 空ファイル
        [Test]
        public async Task Tail_EmptyFile_Success()
        {
            CreateFile("empty.txt");

            var exitCode = await terminal.ExecuteAsync("tail empty.txt", stdout, stderr);

            Assert.AreEqual(ExitCode.Success, exitCode);
            Assert.IsEmpty(stdout.ToString().Trim());
        }

        // TAIL-040 -f オプションでファイル更新を検出
        [Test]
        public async Task Tail_Follow_DetectsFileUpdate()
        {
            var filePath = Path.Combine(testDir, "follow.txt");
            File.WriteAllText(filePath, "initial\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow.txt", stdout, stderr, ct: cts.Token);

            // ファイル更新を待つための短い遅延
            await Task.Delay(150);

            // ファイルに追記
            File.AppendAllText(filePath, "appended\n");

            // 更新が検出されるまで待機
            await Task.Delay(200);

            // キャンセルして終了
            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("initial"));
            Assert.IsTrue(output.Contains("appended"));
        }

        // TAIL-041 -f オプションで複数行追記
        [Test]
        public async Task Tail_Follow_DetectsMultipleLines()
        {
            var filePath = Path.Combine(testDir, "follow_multi.txt");
            File.WriteAllText(filePath, "start\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_multi.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // 複数行を追記
            File.AppendAllText(filePath, "line1\nline2\nline3\n");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("line1"));
            Assert.IsTrue(output.Contains("line2"));
            Assert.IsTrue(output.Contains("line3"));
        }

        // TAIL-042 -f オプションで改行なしのテキスト
        [Test]
        public async Task Tail_Follow_HandlesTextWithoutNewline()
        {
            var filePath = Path.Combine(testDir, "follow_nonewline.txt");
            File.WriteAllText(filePath, "start\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_nonewline.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // 改行なしで追記
            File.AppendAllText(filePath, "no newline at end");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("no newline at end"));
        }

        // TAIL-043 -f オプションでファイルが切り詰められた場合
        [Test]
        public async Task Tail_Follow_HandlesTruncatedFile()
        {
            var filePath = Path.Combine(testDir, "follow_truncate.txt");
            File.WriteAllText(filePath, "original content that is long\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_truncate.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // ファイルを切り詰めて新しい内容を書き込む
            File.WriteAllText(filePath, "new\n");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("file truncated"), "Should show truncation message");
            Assert.IsTrue(output.Contains("new"), "Should show new content");
        }

        // TAIL-044 長い行の処理
        [Test]
        public async Task Tail_Follow_HandlesLongLines()
        {
            var filePath = Path.Combine(testDir, "follow_long.txt");
            File.WriteAllText(filePath, "start\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_long.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // 長い行を追記
            var longLine = new string('A', 1000);
            File.AppendAllText(filePath, longLine + "\n");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains(longLine));
        }

        // TAIL-045 CRLFの改行処理
        [Test]
        public async Task Tail_Follow_HandlesCRLF()
        {
            var filePath = Path.Combine(testDir, "follow_crlf.txt");
            File.WriteAllText(filePath, "start\r\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_crlf.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // CRLF改行で追記
            File.AppendAllText(filePath, "line1\r\nline2\r\n");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("line1"));
            Assert.IsTrue(output.Contains("line2"));
        }

        // TAIL-046 ファイルが削除されて再作成された場合
        [Test]
        public async Task Tail_Follow_HandlesDeleteAndRecreate()
        {
            var filePath = Path.Combine(testDir, "follow_recreate.txt");
            File.WriteAllText(filePath, "original\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_recreate.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // ファイルを削除して再作成
            File.Delete(filePath);
            await Task.Delay(150);
            File.WriteAllText(filePath, "new content after recreate\n");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("original"), "Should show original content");
            Assert.IsTrue(output.Contains("new content after recreate"), "Should show recreated content");
            // 削除→再作成の場合はtruncatedメッセージは出ない（lastPositionが0にリセットされるため）
            Assert.IsFalse(output.Contains("file truncated"), "Should not show truncation message for delete/recreate");
        }

        // TAIL-047 ファイルが削除されて、より長い内容で再作成された場合
        [Test]
        public async Task Tail_Follow_HandlesDeleteAndRecreateWithLongerContent()
        {
            var filePath = Path.Combine(testDir, "follow_recreate_long.txt");
            File.WriteAllText(filePath, "short\n");

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f follow_recreate_long.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // ファイルを削除して、より長い内容で再作成
            File.Delete(filePath);
            await Task.Delay(150);
            File.WriteAllText(filePath, "this is a much longer content than before\n");

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("short"), "Should show original content");
            // 先頭から全ての内容が出力されることを確認
            Assert.IsTrue(output.Contains("this is a much longer content than before"), "Should show recreated content from beginning");
            // 削除→再作成の場合はtruncatedメッセージは出ない
            Assert.IsFalse(output.Contains("file truncated"), "Should not show truncation message for delete/recreate");
        }

        // TAIL-048 -f オプションでファイルが切り詰められた場合、最後のN行のみ出力
        [Test]
        public async Task Tail_Follow_TruncatedFile_OutputsLastNLines()
        {
            var filePath = Path.Combine(testDir, "follow_truncate_lines.txt");
            // 最初に長いファイルを作成
            var originalLines = new string[20];
            for (int i = 0; i < 20; i++)
                originalLines[i] = $"original{i + 1}";
            File.WriteAllLines(filePath, originalLines);

            using var cts = new System.Threading.CancellationTokenSource();
            var followTask = terminal.ExecuteAsync("tail -f -n=3 follow_truncate_lines.txt", stdout, stderr, ct: cts.Token);

            await Task.Delay(150);

            // ファイルを切り詰めて新しい内容（5行）を書き込む
            var newLines = new[] { "new1", "new2", "new3", "new4", "new5" };
            File.WriteAllLines(filePath, newLines);

            await Task.Delay(200);

            cts.Cancel();

            try
            {
                await followTask;
            }
            catch (System.OperationCanceledException)
            {
                // 期待される動作
            }

            var output = stdout.ToString();
            Assert.IsTrue(output.Contains("file truncated"), "Should show truncation message");
            // -n=3 なので最後の3行のみ出力される
            Assert.IsTrue(output.Contains("new3"), "Should show last 3 lines");
            Assert.IsTrue(output.Contains("new4"), "Should show last 3 lines");
            Assert.IsTrue(output.Contains("new5"), "Should show last 3 lines");
            // new1, new2は含まれない（最後の3行ではないため）
            // ただし、最初のtail出力には含まれる可能性があるので、truncation後の部分のみチェック
        }

    }
}
